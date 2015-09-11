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
using NetFreeSwitch.Framework.Net.Buffers;

namespace NetFreeSwitch.Framework.Net.Channels {
    /// <summary>
    ///     Creates a <see cref="TcpChannel" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Allows you to provide your own custom channels to be able to control the IO operations that this library uses.
    ///     </para>
    /// </remarks>
    public class TcpChannelFactory : ITcpChannelFactory {
        /// <summary>
        ///     Create a new channel
        /// </summary>
        /// <param name="readBuffer">Buffer which should be used when reading from the socket</param>
        /// <param name="encoder">Used to encode outgoing data</param>
        /// <param name="decoder">Used to decode incoming data</param>
        /// <returns>Created channel</returns>
        public ITcpChannel Create(IBufferSlice readBuffer, IMessageEncoder encoder, IMessageDecoder decoder) {
            var channel = new TcpChannel(readBuffer, encoder, decoder);
            if (OutboundMessageQueueFactory != null)
                channel.OutboundMessageQueue = OutboundMessageQueueFactory();
            return channel;
        }

        /// <summary>
        ///     Create a new queue which is used to store outbound messages in the created channel.
        /// </summary>
        /// <returns>Factory method</returns>
        public Func<IMessageQueue> OutboundMessageQueueFactory { get; set; }
    }
}
