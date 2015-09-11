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
    ///     Used to create secure channels for our library.
    /// </summary>
    public class SecureTcpChannelFactory : ITcpChannelFactory {
        private readonly ISslStreamBuilder _sslStreamBuilder;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SecureTcpChannelFactory" /> class.
        /// </summary>
        /// <param name="sslStreamBuilder">The SSL stream builder.</param>
        /// <exception cref="System.ArgumentNullException">sslStreamBuilder</exception>
        public SecureTcpChannelFactory(ISslStreamBuilder sslStreamBuilder) {
            if (sslStreamBuilder == null) throw new ArgumentNullException("sslStreamBuilder");

            _sslStreamBuilder = sslStreamBuilder;
        }

        /// <summary>
        ///     Create a new channel
        /// </summary>
        /// <param name="readBuffer">Buffer which should be used when reading from the socket</param>
        /// <param name="encoder">Used to encode outgoing data</param>
        /// <param name="decoder">Used to decode incoming data</param>
        /// <returns>Created channel</returns>
        public ITcpChannel Create(IBufferSlice readBuffer, IMessageEncoder encoder, IMessageDecoder decoder) {
            var channel = new SecureTcpChannel(readBuffer, encoder, decoder, _sslStreamBuilder);
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
