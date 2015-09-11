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
using System.IO;
using NetFreeSwitch.Framework.Net.Channels;

namespace NetFreeSwitch.Framework.Net.Protocols {
    /// <summary>
    ///     Used by <see cref="ChannelTcpListener.ClientConnected" />.
    /// </summary>
    public class ClientConnectedEventArgs : EventArgs {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ClientConnectedEventArgs" /> class.
        /// </summary>
        /// <param name="channel">The channel.</param>
        public ClientConnectedEventArgs(ITcpChannel channel) {
            if (channel == null) throw new ArgumentNullException("channel");
            Channel = channel;
            MayConnect = true;
            SendResponse = true;
        }

        /// <summary>
        ///     Channel for the connected client
        /// </summary>
        public ITcpChannel Channel { get; private set; }

        /// <summary>
        ///     Response (if the client may not connect)
        /// </summary>
        public Stream Response { get; private set; }

        /// <summary>
        ///     Determines if the client may connect.
        /// </summary>
        public bool MayConnect { get; private set; }

        /// <summary>
        ///     The library should send a response.
        /// </summary>
        /// <value>
        ///     Default is <c>true</c>.
        /// </value>
        public bool SendResponse { get; set; }

        /// <summary>
        ///     Cancel connection, will make the listener close it.
        /// </summary>
        public void CancelConnection() { MayConnect = false; }

        /// <summary>
        ///     Close the listener, but send a response (you are yourself responsible of encoding it to a message)
        /// </summary>
        /// <param name="response">Stream with encoded message (which can be sent as-is).</param>
        public void CancelConnection(Stream response) {
            if (response == null) throw new ArgumentNullException("response");
            Response = response;
            MayConnect = false;
        }
    }
}
