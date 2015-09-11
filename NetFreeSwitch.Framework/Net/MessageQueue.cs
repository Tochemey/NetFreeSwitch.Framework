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

namespace NetFreeSwitch.Framework.Net {
    /// <summary>
    ///     Used to enqueue outbound messages
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Implemented using a ConcurrentQueue.
    ///     </para>
    /// </remarks>
    public class MessageQueue : IMessageQueue {
        private readonly ConcurrentQueue<object> _outboundMessages = new ConcurrentQueue<object>();

        /// <summary>
        ///     Enqueue a message
        /// </summary>
        /// <param name="message">message to enqueue</param>
        /// <remarks>
        ///     <para>
        ///         Messages do not have to be placed in order, place them as they should be sent out.
        ///     </para>
        /// </remarks>
        public void Enqueue(object message) {
            if (message == null) throw new ArgumentNullException("message");
            _outboundMessages.Enqueue(message);
        }

        /// <summary>
        ///     Get the next message that should be sent
        /// </summary>
        /// <param name="msg">Message to send</param>
        /// <returns><c>true</c> if there was a message to send.</returns>
        public bool TryDequeue(out object msg) { return _outboundMessages.TryDequeue(out msg); }
    }
}
