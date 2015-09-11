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
using System.Net;
using NetFreeSwitch.Framework.Net.Channels;

namespace NetFreeSwitch.Framework.Net.Protocols {
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
