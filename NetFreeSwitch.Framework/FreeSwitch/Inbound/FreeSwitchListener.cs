/*
Licensed to the Apache Software Foundation (ASF) under one
or more contributor license agreements.  See the NOTICE file
distributed with this work for additional information
regarding copyright ownership.  The ASF licenses this file
to you under the Apache License, Version 2.0 (the
"License"); you may not use this file except in compliance
with the License.  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing,
software distributed under the License is distributed on an
"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, either express or implied.  See the License for the
specific language governing permissions and limitations
under the License.
*/

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using NetFreeSwitch.Framework.FreeSwitch.Codecs;
using NetFreeSwitch.Framework.Net;
using NetFreeSwitch.Framework.Net.Buffers;
using NetFreeSwitch.Framework.Net.Channels;
using NetFreeSwitch.Framework.Net.Protocols;
using NLog;

namespace NetFreeSwitch.Framework.FreeSwitch.Inbound {
    /// <summary>
    ///     Tcp Server listening to any FreeSwich connection.
    ///     Very useful when implementing FreeSwitch mod_event_socket outbound mode.
    /// </summary>
    public class FreeSwitchListener : IMessagingListener {
        private Logger _log = LogManager.GetCurrentClassLogger();
        private readonly ConcurrentStack<ITcpChannel> _channels = new ConcurrentStack<ITcpChannel>();
        private IBufferSlicePool _bufferPool;
        private ChannelTcpListenerConfiguration _configuration;
        private TcpListener _listener;
        private MessageHandler _messageReceived;
        private MessageHandler _messageSent;

        /// <summary>
        /// </summary>
        /// <param name="configuration"></param>
        public FreeSwitchListener(ChannelTcpListenerConfiguration configuration) {
            if (configuration == null) throw new ArgumentNullException("configuration");

            Configure(configuration);
            ChannelFactory = new TcpChannelFactory();
        }

        /// <summary>
        /// </summary>
        public FreeSwitchListener() {
            Configure(new ChannelTcpListenerConfiguration(() => new FreeSwitchDecoder(), () => new FreeSwitchEncoder()));
            ChannelFactory = new TcpChannelFactory();
        }

        public event EventHandler<ClientConnectedEventArgs> ClientConnected;

        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;

        /// <summary>
        ///     An internal error occurred
        /// </summary>
        public event EventHandler<ErrorEventArgs> ListenerError = delegate { };

        public ITcpChannelFactory ChannelFactory { get; set; }

        /// <summary>
        ///     Port that the server is listening on.
        /// </summary>
        /// <remarks>
        ///     You can use port <c>0</c> in <see cref="Start" /> to let the OS assign a port. This method will then give you the
        ///     assigned port.
        /// </remarks>
        public int LocalPort
        {
            get
            {
                if (_listener == null)
                    return -1;

                return ((IPEndPoint) _listener.LocalEndpoint).Port;
            }
        }

        /// <summary>
        ///     Delegate invoked when a new message is received
        /// </summary>
        public MessageHandler MessageReceived { get { return _messageReceived; } set { _messageReceived = value ?? delegate { }; } }

        /// <summary>
        ///     Delegate invoked when a message has been sent to the remote end point
        /// </summary>
        public MessageHandler MessageSent { get { return _messageSent; } set { _messageSent = value ?? delegate { }; } }

        /// <summary>
        ///     Start this listener.
        /// </summary>
        /// <remarks>
        ///     This also pre-configures 20 channels that can be used and reused during the lifetime of
        ///     this listener.
        /// </remarks>
        /// <param name="address">Address to accept connections on</param>
        /// <param name="port">Port to use. Set to <c>0</c> to let the OS decide which port to use. </param>
        /// <seealso cref="LocalPort" />
        public virtual void Start(IPAddress address, int port) {
            if (port < 0)
                throw new ArgumentOutOfRangeException("port", port, "Port must be 0 or more.");
            if (_listener != null)
                throw new InvalidOperationException("Already listening.");

            _listener = new TcpListener(address, port);
            _listener.AllowNatTraversal(true);
            _listener.Start();

            for (var i = 0; i < 20; i++) {
                var decoder = _configuration.DecoderFactory();
                var encoder = _configuration.EncoderFactory();
                var channel = ChannelFactory.Create(_bufferPool.Pop(), encoder, decoder);
                _channels.Push(channel);
            }

            _listener.BeginAcceptSocket(OnAcceptSocket, null);
        }

        /// <summary>
        ///     Stop the listener.
        /// </summary>
        public virtual async void Stop() {
            // Let us do some cleaning
            foreach (ITcpChannel tcpChannel in _channels) await tcpChannel.CloseAsync();
            _channels.Clear();
            _listener.Stop();
        }

        /// <summary>
        ///     To allow the sub classes to configure this class in their constructors.
        /// </summary>
        /// <param name="configuration"></param>
        protected void Configure(ChannelTcpListenerConfiguration configuration) {
            _bufferPool = configuration.BufferPool;
            _configuration = configuration;
        }

        /// <summary>
        ///     A client has connected (nothing has been sent or received yet)
        /// </summary>
        /// <param name="channel">Channel which we created for the remote socket.</param>
        /// <returns></returns>
        protected virtual ClientConnectedEventArgs OnClientConnected(ITcpChannel channel) {
            var args = new ClientConnectedEventArgs(channel);
            ClientConnected(this, args);
            return args;
        }

        /// <summary>
        ///     A client has disconnected
        /// </summary>
        /// <param name="channel">Channel representing the client that disconnected</param>
        /// <param name="exception">
        ///     Exception which was used to detect disconnect (<c>SocketException</c> with status
        ///     <c>Success</c> is created for graceful disconnects)
        /// </param>
        protected virtual void OnClientDisconnected(ITcpChannel channel, Exception exception) { ClientDisconnected(this, new ClientDisconnectedEventArgs(channel, exception)); }

        /// <summary>
        ///     Receive a new message from the specified client
        /// </summary>
        /// <param name="source">Channel for the client</param>
        /// <param name="msg">Message (as decoded by the specified <see cref="IMessageDecoder" />).</param>
        protected virtual void OnMessage(ITcpChannel source, object msg) { _messageReceived(source, msg); }

        private void OnAcceptSocket(IAsyncResult ar) {
            try {
                var socket = _listener.EndAcceptSocket(ar);
                socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(true, 1));
                ITcpChannel channel;
                if (!_channels.TryPop(out channel)) {
                    var decoder = _configuration.DecoderFactory();
                    var encoder = _configuration.EncoderFactory();
                    channel = ChannelFactory.Create(_bufferPool.Pop(), encoder, decoder);
                }

                channel.Disconnected += OnChannelDisconnect;
                channel.MessageReceived += OnMessage;
                channel.Assign(socket);

                var args = OnClientConnected(channel);
                if (!args.MayConnect) {
                    if (args.Response != null)
                        channel.Send(args.Response);
                    channel.Close();
                    return;
                }
            }
            catch (Exception exception) {
                ListenerError(this, new ErrorEventArgs(exception));
            }

            if (_listener != null) _listener.BeginAcceptSocket(OnAcceptSocket, null);
        }

        private void OnChannelDisconnect(ITcpChannel source, Exception exception) {
            OnClientDisconnected(source, exception);
            source.Cleanup();
            _channels.Push(source);
        }
    }
}