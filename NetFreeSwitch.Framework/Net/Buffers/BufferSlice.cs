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

namespace NetFreeSwitch.Framework.Net.Buffers {
    /// <summary>
    ///     Used to slice a larger buffer into smaller chunks.
    /// </summary>
    public class BufferSlice : IBufferSlice {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BufferSlice" /> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">Start offset in buffer.</param>
        /// <param name="count">Number of bytes allocated for this slice..</param>
        /// <exception cref="System.ArgumentNullException">buffer</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">offset;Offset+Count must be less than the buffer length.</exception>
        public BufferSlice(byte[] buffer, int offset, int count) {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException("offset", offset, "Offset+Count must be less than the buffer length.");

            Capacity = count;
            Offset = offset;
            Buffer = buffer;
        }

        /// <summary>
        ///     Where this slice starts
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        ///     AMount of bytes allocated for this slice.
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        ///     Buffer that this slice is in.
        /// </summary>
        public byte[] Buffer { get; private set; }
    }
}
