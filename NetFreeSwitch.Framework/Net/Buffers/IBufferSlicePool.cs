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

namespace NetFreeSwitch.Framework.Net.Buffers {
    /// <summary>
    ///     Represents a part of a larger byte buffer.
    /// </summary>
    public interface IBufferSlicePool {
        /// <summary>
        ///     Pop a new slice
        /// </summary>
        /// <returns>New slice</returns>
        /// <exception cref="PoolEmptyException">There are no more free slices in the pool.</exception>
        IBufferSlice Pop();

        /// <summary>
        ///     Return a slice to the pool
        /// </summary>
        /// <param name="bufferSlice">Slice retrieved using <see cref="Pop" />.</param>
        void Push(IBufferSlice bufferSlice);
    }
}
