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

namespace NetFreeSwitch.Framework.Net {
    /// <summary>
    ///     Used to enqueue outbound messages
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         As a socket must complete it's operations before the next message can be send we need to be able to queue
    ///         outbound messages
    ///         so that we have a chance to complete IO operations for each message.
    ///     </para>
    ///     <para>
    ///         Without this, the socket would probably throw errors on us due toe the asynchronous nature of this library
    ///         implementation
    ///     </para>
    ///     <para>
    ///         You can create your own implementation which could allow for prioritizing or so that messages are sent in the
    ///         same order as their corresponding request comes in (if the message processors are not completed in order)
    ///     </para>
    /// </remarks>
    public interface IMessageQueue {
        /// <summary>
        ///     Enqueue a message
        /// </summary>
        /// <param name="message">message to enqueue</param>
        /// <remarks>
        ///     <para>
        ///         Messages do not have to be placed in order, place them as they should be sent out.
        ///     </para>
        ///     <para>
        ///         You must be able to enqueue all messages as the library use some internal messages for different operations
        ///         (like being able to close when all messages have been sent)
        ///     </para>
        /// </remarks>
        void Enqueue(object message);

        /// <summary>
        ///     Get the next message that should be sent
        /// </summary>
        /// <param name="msg">Message to send</param>
        /// <returns><c>true</c> if there was a message to send.</returns>
        bool TryDequeue(out object msg);
    }
}
