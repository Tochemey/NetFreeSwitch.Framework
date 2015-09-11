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

namespace NetFreeSwitch.Framework.Net {
    /// <summary>
    ///     Decodes incoming bytes into something more useful
    /// </summary>
    public interface IMessageDecoder {
        /// <summary>
        ///     A message has been received.
        /// </summary>
        /// <remarks>
        ///     Do note that streams are being reused by the decoder, so don't try to close it.
        /// </remarks>
        Action<object> MessageReceived { get; set; }

        /// <summary>
        ///     Clear state to allow this decoder to be reused.
        /// </summary>
        void Clear();

        /// <summary>
        ///     We've received bytes from the socket. Build a message out of them.
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <remarks></remarks>
        void ProcessReadBytes(ISocketBuffer buffer);
    }
}
