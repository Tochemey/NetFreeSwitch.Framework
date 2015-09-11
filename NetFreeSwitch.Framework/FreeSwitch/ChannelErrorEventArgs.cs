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
using NetFreeSwitch.Framework.Net.Channels;
using NetFreeSwitch.Framework.Net.Protocols;

namespace NetFreeSwitch.Framework.FreeSwitch {
    public class ChannelErrorEventArgs : EventArgs{
        /// <summary>
        ///     Initializes a new instance of the <see cref="ClientDisconnectedEventArgs" /> class.
        /// </summary>
        /// <param name="channel">The channel that disconnected.</param>
        /// <param name="exception">The exception that was caught.</param>
        public ChannelErrorEventArgs(ITcpChannel channel, Exception exception)
        {
            if (channel == null) throw new ArgumentNullException("channel");
            if (exception == null) throw new ArgumentNullException("exception");

            Channel = channel;
            Exception = exception;
        }

        /// <summary>
        ///     Channel that was disconnected
        /// </summary>
        public ITcpChannel Channel { get; private set; }

        /// <summary>
        ///     Exception that was caught (is SocketException if the connection failed or if the remote end point disconnected).
        /// </summary>
        /// <remarks>
        ///     <c>SocketException</c> with status <c>Success</c> is created for graceful disconnects.
        /// </remarks>
        public Exception Exception { get; private set; }

    }
}
