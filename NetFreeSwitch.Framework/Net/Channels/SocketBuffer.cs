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

using NetFreeSwitch.Framework.Net.Buffers;

namespace NetFreeSwitch.Framework.Net.Channels {
    /// <summary>
    ///     USed for the SecureTcpChannel as we can't use the Async socket methods with it :(
    /// </summary>
    internal class SocketBuffer : ISocketBuffer {
        public SocketBuffer() {
            Buffer = new byte[65535];
            Capacity = 65535;
            Offset = 0;
        }

        public SocketBuffer(IBufferSlice readBuffer) {
            Buffer = readBuffer.Buffer;
            Capacity = readBuffer.Capacity;
            BaseOffset = readBuffer.Offset;
            Offset = readBuffer.Offset;
        }

        /// <summary>
        ///     an object which can be used by you to keep track of what's being sent and received.
        /// </summary>
        public object UserToken { get; set; }

        public int BytesTransferred { get; set; }
        public int Count { get; set; }
        public int Capacity { get; set; }
        public byte[] Buffer { get; set; }
        public int BaseOffset { get; set; }
        public int Offset { get; set; }

        public void SetBuffer(int offset, int count) {
            Offset = offset;
            Count = count;
        }

        public void SetBuffer(byte[] buffer, int offset, int count, int capacity) {
            Buffer = buffer;
            Count = count;
            Offset = offset;
            Capacity = capacity;
        }

        public void SetBuffer(byte[] buffer, int offset, int count) {
            Buffer = buffer;
            Count = count;
            Offset = offset;
            Capacity = count;
        }
    }
}
