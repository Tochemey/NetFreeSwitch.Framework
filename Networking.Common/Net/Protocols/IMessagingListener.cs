﻿using System;
using System.Net;
using Networking.Common.Net.Channels;

namespace Networking.Common.Net.Protocols {
    /// <summary>
    ///     Used to listen on new messages
    /// </summary>
    public interface IMessagingListener {
        /// <summary>
        ///     Used to create channels. Default is <see cref="TcpChannelFactory" />.
        /// </summary>
        ITcpChannelFactory ChannelFactory { get; set; }

        /// <summary>
        ///     Delegate invoked when a new message is received
        /// </summary>
        MessageHandler MessageReceived { get; set; }

        /// <summary>
        ///     Delegate invoked when a message has been sent to the remote end point
        /// </summary>
        MessageHandler MessageSent { get; set; }

        /// <summary>
        ///     A client has connected (nothing has been sent or received yet)
        /// </summary>
        event EventHandler<ClientConnectedEventArgs> ClientConnected;

        /// <summary>
        ///     A client has disconnected
        /// </summary>
        event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;

        /// <summary>
        ///     Start this listener
        /// </summary>
        /// <param name="address">Address to accept connections on</param>
        /// <param name="port">Port to use. Set to <c>0</c> to let the OS decide which port to use. </param>
        /// <seealso cref="ChannelTcpListener.LocalPort" />
        void Start(IPAddress address, int port);

        /// <summary>
        ///     Stop the listener.
        /// </summary>
        void Stop();
    }
}