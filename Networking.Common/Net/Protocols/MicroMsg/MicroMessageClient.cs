﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Networking.Common.Net.Buffers;
using Networking.Common.Net.Channels;
using Networking.Common.Net.Protocols.Serializers;

namespace Networking.Common.Net.Protocols.MicroMsg {
    /// <summary>
    ///     A client implementation for transferring objects over the MicroMessage protocol
    /// </summary>
    public class MicroMessageClient {
        private readonly SocketAsyncEventArgs _args = new SocketAsyncEventArgs();
        private readonly MicroMessageDecoder _decoder;
        private readonly MicroMessageEncoder _encoder;
        private readonly ClientSideSslStreamBuilder _sslStreamBuilder;
        private ITcpChannel _channel;
        private TaskCompletionSource<IPEndPoint> _connectCompletionSource;
        private TaskCompletionSource<object> _readCompletionSource;
        private TaskCompletionSource<object> _sendCompletionSource;
        private Socket _socket;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MicroMessageClient" /> class.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        public MicroMessageClient(IMessageSerializer serializer) {
            _decoder = new MicroMessageDecoder(serializer);
            _encoder = new MicroMessageEncoder(serializer);
            _decoder.MessageReceived = OnMessageReceived;
            _args.Completed += OnConnect;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MicroMessageClient" /> class.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="sslStreamBuilder">The SSL stream builder.</param>
        public MicroMessageClient(IMessageSerializer serializer, ClientSideSslStreamBuilder sslStreamBuilder) {
            _sslStreamBuilder = sslStreamBuilder;
            _decoder = new MicroMessageDecoder(serializer);
            _encoder = new MicroMessageEncoder(serializer);
            _decoder.MessageReceived = OnMessageReceived;
            _args.Completed += OnConnect;
        }

        private ITcpChannel CreateChannel() {
            if (_channel != null)
                return _channel;

            if (_sslStreamBuilder != null) _channel = new SecureTcpChannel(new BufferSlice(new byte[65535], 0, 65535), _encoder, _decoder, _sslStreamBuilder);
            else _channel = new TcpChannel(new BufferSlice(new byte[65535], 0, 65535), _encoder, _decoder);

            _channel.Disconnected = OnDisconnect;
            _channel.MessageSent = OnSendCompleted;
            _channel.MessageReceived = OnChannelMessageReceived;
            return _channel;
        }

        /// <summary>
        ///     Connect to server
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">
        ///     Socket is already connected
        ///     or
        ///     There is already a pending connect.
        /// </exception>
        public Task ConnectAsync(IPAddress address, int port) {
            if (_socket != null)
                throw new InvalidOperationException("Socket is already connected");
            if (_connectCompletionSource != null)
                throw new InvalidOperationException("There is already a pending connect.");

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _args.RemoteEndPoint = new IPEndPoint(address, port);
            var isPending = _socket.ConnectAsync(_args);

            _connectCompletionSource = new TaskCompletionSource<IPEndPoint>();
            if (!isPending) _connectCompletionSource.SetResult((IPEndPoint) _socket.RemoteEndPoint);

            return _connectCompletionSource.Task;
        }

        /// <summary>
        ///     Receive an object
        /// </summary>
        /// <returns>completion task</returns>
        public Task<object> ReceiveAsync() {
            if (_readCompletionSource != null)
                throw new InvalidOperationException("There is already a pending receive operation.");

            _readCompletionSource = new TaskCompletionSource<object>();
            _readCompletionSource.Task.ConfigureAwait(false);

            return _readCompletionSource.Task;
        }

        /// <summary>
        ///     Send an object
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>completion task (completed once the message have been delivered).</returns>
        /// <remarks>
        ///     <para>All objects are enqueued and sent in order as soon as possible</para>
        /// </remarks>
        public Task SendAsync(object message) {
            if (message == null) throw new ArgumentNullException("message");
            if (_sendCompletionSource != null)
                throw new InvalidOperationException("There is already a pending send operation.");

            _sendCompletionSource = new TaskCompletionSource<object>();
            _sendCompletionSource.Task.ConfigureAwait(false);

            _channel.Send(message);
            return _sendCompletionSource.Task;
        }

        private void OnChannelMessageReceived(ITcpChannel channel, object message) {
            _readCompletionSource.SetResult(message);
            _readCompletionSource = null;
        }

        private void OnConnect(object sender, SocketAsyncEventArgs e) {
            if (e.SocketError != SocketError.Success) {
                _connectCompletionSource.SetException(new SocketException((int) e.SocketError));
                _connectCompletionSource = null;
                return;
            }

            try {
                if (_channel == null)
                    _channel = CreateChannel();

                _channel.Assign(_socket);
            }
            catch (Exception exception) {
                _connectCompletionSource.SetException(exception);
                _connectCompletionSource = null;
                return;
            }

            _connectCompletionSource.SetResult((IPEndPoint) _socket.RemoteEndPoint);
            _connectCompletionSource = null;
        }

        private void OnDisconnect(ITcpChannel arg1, Exception arg2) {
            _socket = null;

            if (_sendCompletionSource != null) {
                _sendCompletionSource.SetException(arg2);
                _sendCompletionSource = null;
            }

            if (_readCompletionSource != null) {
                _readCompletionSource.SetException(arg2);
                _readCompletionSource = null;
            }
        }

        private void OnMessageReceived(object obj) { _readCompletionSource.SetResult(obj); }

        private void OnSendCompleted(ITcpChannel channel, object sentMessage) {
            _sendCompletionSource.SetResult(sentMessage);
            _sendCompletionSource = null;
        }
    }
}