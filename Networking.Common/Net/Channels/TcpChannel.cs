﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Networking.Common.Net.Buffers;

namespace Networking.Common.Net.Channels {
    /// <summary>
    ///     Represents a socket connection between two end points.
    /// </summary>
    public class TcpChannel : ITcpChannel {
        private static readonly object CloseMessage = new object();
        private readonly SemaphoreSlim _closeEvent = new SemaphoreSlim(0, 1);
        private readonly IMessageDecoder _decoder;
        private readonly IMessageEncoder _encoder;
        private readonly SocketAsyncEventArgs _readArgs;
        private readonly SocketAsyncEventArgsWrapper _readArgsWrapper;
        private readonly SocketAsyncEventArgs _writeArgs;
        private readonly SocketAsyncEventArgsWrapper _writeArgsWrapper;
        private object _currentOutboundMessage;
        private DisconnectHandler _disconnectAction;
        private MessageHandler _messageReceived;
        private IMessageQueue _outboundMessages = new MessageQueue();
        private MessageHandler _sendCompleteAction;
        private Socket _socket;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TcpChannel" /> class.
        /// </summary>
        /// <param name="readBuffer">Buffer used for our reading.</param>
        /// <param name="encoder">Used to encode messages before they are put in the MicroMessage body of outbound messages.</param>
        /// <param name="decoder">
        ///     Used to decode the body of incoming MicroMessages. The <c>MessageReceived</c> delegate will be
        ///     overridden by this class.
        /// </param>
        public TcpChannel(IBufferSlice readBuffer, IMessageEncoder encoder, IMessageDecoder decoder) {
            if (readBuffer == null) throw new ArgumentNullException("readBuffer");
            if (encoder == null) throw new ArgumentNullException("encoder");
            if (decoder == null) throw new ArgumentNullException("decoder");

            _readArgs = new SocketAsyncEventArgs();
            _readArgs.SetBuffer(readBuffer.Buffer, readBuffer.Offset, readBuffer.Capacity);
            _readArgs.Completed += OnReadCompleted;
            _readArgsWrapper = new SocketAsyncEventArgsWrapper(_readArgs);

            _encoder = encoder;
            _decoder = decoder;
            _decoder.MessageReceived = OnMessageReceived;

            _writeArgs = new SocketAsyncEventArgs();
            _writeArgs.Completed += OnSendCompleted;
            _writeArgsWrapper = new SocketAsyncEventArgsWrapper(_writeArgs);

            _sendCompleteAction = (channel, message) => { };
            _disconnectAction = (channel, exception) => { };
            ChannelFailure = (channel, error) => HandleDisconnect(SocketError.ProtocolNotSupported);

            RemoteEndpoint = EmptyEndpoint.Instance;
            ChannelId = GuidFactory.Create().ToString();
            Data = new ChannelData();
        }

        /// <summary>
        ///     Used to enqueue outbound messages (to support asynchronous handling, i.e. enqueue more messages before the current
        ///     one has been sent)
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This property exists so that you can switch implementation.  This is used by the HttpListener so that we can
        ///         add support
        ///         for message pipelining
        ///     </para>
        /// </remarks>
        public IMessageQueue OutboundMessageQueue
        {
            get { return _outboundMessages; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _outboundMessages = value;
            }
        }

        /// <summary>
        ///     Channel got disconnected
        /// </summary>
        public DisconnectHandler Disconnected
        {
            get { return _disconnectAction; }

            set
            {
                if (value == null)
                    _disconnectAction = (x, e) => { };
                else
                    _disconnectAction = value;
            }
        }

        /// <summary>
        ///     Channel received a new message
        /// </summary>
        public MessageHandler MessageReceived
        {
            get { return _messageReceived; }
            set
            {
                if (value == null)
                    throw new ArgumentException("You must have a MessageReceived delegate");


                _messageReceived = value;
            }
        }

        /// <summary>
        ///     Channel has sent a message
        /// </summary>
        public MessageHandler MessageSent
        {
            get { return _sendCompleteAction; }

            set
            {
                if (value == null) {
                    _sendCompleteAction = (x, y) => { };
                    return;
                }

                _sendCompleteAction = value;
            }
        }

        /// <summary>
        ///     The channel failed to complete an IO operation
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The handler MUST close the connection once a reply has been sent.
        ///     </para>
        /// </remarks>
        public ChannelFailureHandler ChannelFailure { get; set; }

        /// <summary>
        ///     Gets address of the connected end point.
        /// </summary>
        public EndPoint RemoteEndpoint { get; private set; }

        /// <summary>
        ///     Identity of this channel
        /// </summary>
        /// <remarks>
        ///     Must be unique within a server.
        /// </remarks>
        public string ChannelId { get; }

        /// <summary>
        ///     Assign a socket to this channel
        /// </summary>
        /// <param name="socket">Connected socket</param>
        /// <remarks>
        ///     the channel will start receive new messages as soon as you've called assign.
        ///     <para>
        ///         You must have specified a <see cref="MessageReceived" /> delegate first.
        ///     </para>
        /// </remarks>
        public void Assign(Socket socket) {
            if (socket == null) throw new ArgumentNullException("socket");
            if (_messageReceived == null)
                throw new InvalidOperationException("You must have set a MessageReceived delegate first.");

            _socket = socket;
            RemoteEndpoint = socket.RemoteEndPoint;
            IsConnected = true;
            ReadAsync();
        }

        /// <summary>
        ///     Gets if channel is connected
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        ///     Send a new message
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <remarks>
        ///     <para>
        ///         Outbound messages are enqueued and sent in order.
        ///     </para>
        ///     <para>
        ///         You may enqueue <c>byte[]</c> arrays or <see cref="Stream" />  objects. They will not be serialized but
        ///         MicroMessage framed directly.
        ///     </para>
        /// </remarks>
        public void Send(object message) {
            if (_socket.Connected) {
                lock (_outboundMessages) {
                    if (_currentOutboundMessage != null) {
                        _outboundMessages.Enqueue(message);
                        return;
                    }

                    _currentOutboundMessage = message;
                }
            }

            SendCurrent();
        }

        /// <summary>
        ///     Can be used to store information in the channel so that you can access it for later requests.
        /// </summary>
        /// <remarks>
        ///     <para>All data is lost when the channel is closed.</para>
        /// </remarks>
        public IChannelData Data { get; set; }

        /// <summary>
        ///     Pre processes incoming bytes before they are passed to the message builder.
        /// </summary>
        /// <remarks>
        ///     Can be used if you for instance use a custom authentication mechanism which needs to process incoming
        ///     bytes.
        /// </remarks>
        public BufferPreProcessorHandler BufferPreProcessor { get; set; }

        /// <summary>
        ///     Signal channel to close.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Will wait for all data to be sent before closing.
        ///     </para>
        /// </remarks>
        public void Close() {
            Send(CloseMessage);
            _closeEvent.Wait(5000);
            IsConnected = false;
        }

        /// <summary>
        ///     Cleanup everything so that the channel can be reused.
        /// </summary>
        public void Cleanup() {
            _encoder.Clear();
            _decoder.Clear();
            _currentOutboundMessage = null;
            _socket = null;
            RemoteEndpoint = EmptyEndpoint.Instance;
            IsConnected = false;
            if (_closeEvent.CurrentCount == 1)
                _closeEvent.Wait();

            if (Data != null)
                Data.Clear();

            object msg;
            while (_outboundMessages.TryDequeue(out msg)) { }
        }

        /// <summary>
        ///     Signal channel to close.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Will wait for all data to be sent before closing.
        ///     </para>
        /// </remarks>
        public Task CloseAsync() {
            Send(CloseMessage);
            var t = _closeEvent.WaitAsync(5000);

            // release again so that we can take reuse it internally
            t.ContinueWith(x => {
                _closeEvent.Release();
                IsConnected = false;
            });

            return t;
        }

        /// <summary>
        ///     Detected a disconnect
        /// </summary>
        /// <param name="socketError">ProtocolNotSupported = decoder failure.</param>
        private void HandleDisconnect(SocketError socketError) {
            try {
                _socket.Close();
                IsConnected = false;
                _disconnectAction(this, new SocketException((int) socketError));
            }
            catch (Exception exception) {
                ChannelFailure(this, exception);
            }
        }

        /// <summary>
        ///     Detected a disconnect
        /// </summary>
        /// <param name="socketError">ProtocolNotSupported = decoder failure.</param>
        /// <param name="exception">Why we got disconnected</param>
        private void HandleDisconnect(SocketError socketError, Exception exception) {
            try {
                _socket.Close();
                IsConnected = false;
                _disconnectAction(this, exception);
            }
            catch (Exception ex) {
                ChannelFailure(this, ex);
            }
        }

        private void OnMessageReceived(object obj) {
            //TODO: Wrap to catch app exceptions
            _messageReceived(this, obj);
        }

        private void OnReadCompleted(object sender, SocketAsyncEventArgs e) {
            if (e.BytesTransferred == 0
                || e.SocketError != SocketError.Success) {
                HandleDisconnect(e.SocketError);
                return;
            }

            if (BufferPreProcessor != null) {
                var read = BufferPreProcessor(this, _readArgsWrapper);
                if (read > 0) {
                    var newCount = _readArgsWrapper.BytesTransferred - read;
                    var newOffset = _readArgsWrapper.Offset + read;
                    _readArgsWrapper.SetBuffer(newOffset, newCount);
                    _readArgsWrapper.BytesTransferred -= read;
                }
            }

            try {
                // pre processor can have read everything
                if (_readArgsWrapper.BytesTransferred > 0)
                    _decoder.ProcessReadBytes(_readArgsWrapper);
            }
            catch (Exception exception) {
                Console.WriteLine(exception);
                ChannelFailure(this, exception);

                // event handler closed the socket.
                if (!_socket.Connected)
                    return;
            }

            ReadAsync();
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs e) {
            if (e.SocketError != SocketError.Success
                || e.BytesTransferred == 0) {
                // ignore errors, let the receiving end take care of that.
                return;
            }

            var isComplete = _encoder.OnSendCompleted(e.BytesTransferred);
            if (!isComplete) {
                _encoder.Send(_writeArgsWrapper);
                var isPending = _socket.SendAsync(_writeArgs);
                if (!isPending)
                    OnSendCompleted(this, _writeArgs);
                return;
            }

            var msg = _currentOutboundMessage;

            lock (_outboundMessages) {
                if (!_outboundMessages.TryDequeue(out _currentOutboundMessage)) {
                    _currentOutboundMessage = null;
                    _sendCompleteAction(this, msg);
                    return;
                }
            }

            _sendCompleteAction(this, msg);
            SendCurrent();
        }

        private void ReadAsync() {
            try {
                var isPending = _socket.ReceiveAsync(_readArgs);
                if (!isPending) OnReadCompleted(_socket, _readArgs);
            }
            catch (Exception e) {
                HandleDisconnect(SocketError.ConnectionReset, e);
            }
        }

        private void SendCurrent() {
            // Allows us to send everything before closing the connection.
            if (_currentOutboundMessage == CloseMessage) {
                try { _socket.Shutdown(SocketShutdown.Both); }
                catch (Exception e) {
                    HandleDisconnect(SocketError.ConnectionReset, e);
                }
                _currentOutboundMessage = null;
                _closeEvent.Release();
                return;
            }

            _encoder.Prepare(_currentOutboundMessage);
            _encoder.Send(_writeArgsWrapper);
            try {
                var isPending = _socket.SendAsync(_writeArgs);
                if (!isPending)
                    OnSendCompleted(this, _writeArgs);
            }
            catch (Exception e) {
                HandleDisconnect(SocketError.ConnectionReset, e);
            }
        }
    }
}