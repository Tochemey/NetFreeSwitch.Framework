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

namespace NetFreeSwitch.Framework.Net.Buffers {
    /// <summary>
    ///     Creates a large buffer and slices it.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Thread safe, can be used to reuse slices.
    ///     </para>
    /// </remarks>
    public class BufferSlicePool : IBufferSlicePool {
        private readonly byte[] _buffer;
        private readonly ConcurrentStack<IBufferSlice> _slices = new ConcurrentStack<IBufferSlice>();
        private int _sliceSize;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BufferSlicePool" /> class.
        /// </summary>
        /// <param name="sliceSize">How large each slice should be.</param>
        /// <param name="numberOfBuffers">The number of slices.</param>
        public BufferSlicePool(int sliceSize, int numberOfBuffers) {
            _buffer = new byte[sliceSize*numberOfBuffers];
            _sliceSize = sliceSize;
            var offset = 0;
            for (var i = 0; i < numberOfBuffers; i++) {
                _slices.Push(new BufferSlice(_buffer, offset, sliceSize));
                offset += sliceSize;
            }
        }

        /// <summary>
        ///     Get a new slice
        /// </summary>
        /// <returns>Slice</returns>
        /// <exception cref="PoolEmptyException">
        ///     Out of buffers. You are either not releasing used buffers or have allocated fewer
        ///     buffers than allowed number of connected clients.
        /// </exception>
        public IBufferSlice Pop() {
            IBufferSlice pop;
            if (!_slices.TryPop(out pop))
                throw new PoolEmptyException("Out of buffers. You are either not releasing used buffers or have allocated fewer buffers than allowed number of connected clients.");

            return pop;
        }

        /// <summary>
        ///     Enqueue a slice to be able to re-use it later
        /// </summary>
        /// <param name="bufferSlice">Slice to append</param>
        /// <exception cref="System.ArgumentNullException">bufferSlice</exception>
        public void Push(IBufferSlice bufferSlice) {
            if (bufferSlice == null) throw new ArgumentNullException("bufferSlice");
            _slices.Push(bufferSlice);
        }
    }
}
