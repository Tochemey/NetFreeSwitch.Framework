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
using System.Net.Sockets;
using NetFreeSwitch.Framework.Net.Channels;

namespace NetFreeSwitch.Framework.Net {
    /// <summary>
    ///     Message encoders are used to convert objects into binary form so that they can be transferred over a socket.
    /// </summary>
    /// <remarks>
    ///     The format itself is determined by the protocol which is implemented. See all implementations.
    /// </remarks>
    public interface IMessageEncoder {
        /// <summary>
        ///     Prepare the encoder so that the specified object can be encoded next.
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <remarks>
        ///     Can be used to prepare the next message. for instance serialize it etc.
        /// </remarks>
        /// <exception cref="NotSupportedException">Message is of a type that the encoder cannot handle.</exception>
        void Prepare(object message);

        /// <summary>
        ///     Buffer structure used for socket send operations.
        /// </summary>
        /// <param name="buffer">
        ///     Do note that there are not buffer attached to the structure, you have to assign one yourself using
        ///     <see cref="ISocketBuffer.SetBuffer(int,int)" />. This choice was made
        ///     to prevent unnecessary copy operations.
        /// </param>
        /// <remarks>
        ///     The <c>buffer</c> variable is typically a wrapper around <see cref="SocketAsyncEventArgs" />, but may be something
        ///     else if required.
        /// </remarks>
        void Send(ISocketBuffer buffer);

        /// <summary>
        ///     The previous <see cref="Send" /> has just completed.
        /// </summary>
        /// <param name="bytesTransferred"></param>
        /// <remarks><c>true</c> if the message have been sent successfully; otherwise <c>false</c>.</remarks>
        bool OnSendCompleted(int bytesTransferred);

        /// <summary>
        ///     Remove everything used for the last message
        /// </summary>
        void Clear();
    }
}
