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

namespace NetFreeSwitch.Framework.Net {
    /// <summary>
    ///     Configuration for <see cref="ChannelTcpListener" />
    /// </summary>
    public class ChannelTcpListenerConfiguration {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ChannelTcpListenerConfiguration" /> class.
        /// </summary>
        /// <param name="decoderFactory">Used to create an decoder for every new accepted connection.</param>
        /// <param name="encoderFactory">Used to create an encoder for every new accepted connection.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     decoderFactory
        ///     or
        ///     encoderFactory
        /// </exception>
        public ChannelTcpListenerConfiguration(Func<IMessageDecoder> decoderFactory, Func<IMessageEncoder> encoderFactory) {
            if (decoderFactory == null) throw new ArgumentNullException("decoderFactory");
            if (encoderFactory == null) throw new ArgumentNullException("encoderFactory");

            DecoderFactory = decoderFactory;
            EncoderFactory = encoderFactory;
            BufferPool = new BufferSlicePool(65535, 100);
        }

        /// <summary>
        ///     Default constructor
        /// </summary>
        public ChannelTcpListenerConfiguration() { BufferPool = new BufferSlicePool(65535, 100); }

        /// <summary>
        ///     Factory used to produce a decoder for every connected client
        /// </summary>
        public Func<IMessageDecoder> DecoderFactory { get; set; }

        /// <summary>
        ///     Factory used to produce an encoder for every connected client
        /// </summary>
        public Func<IMessageEncoder> EncoderFactory { get; set; }

        /// <summary>
        ///     Pool used to allocate buffers for every client
        /// </summary>
        /// <remarks>
        ///     Each client requires one buffer (for receiving).
        /// </remarks>
        /// <value>
        ///     100 buffers of size 65535 bytes are allocated per default.
        /// </value>
        public IBufferSlicePool BufferPool { get; set; }
    }
}
