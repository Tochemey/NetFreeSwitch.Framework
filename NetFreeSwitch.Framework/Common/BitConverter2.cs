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

namespace NetFreeSwitch.Framework.Common {
    /// <summary>
    ///     Missing <see cref="BitConverter" /> methods.
    /// </summary>
    public static class BitConverter2 {
        /// <summary>
        ///     Copies the int into the pre-allocated buffer.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="offset">The offset.</param>
        public static void GetBytes(int value, byte[] destination, int offset) {
            destination[offset] = (byte) (value & 0xff);
            destination[offset + 1] = (byte) ((value >> 8) & 0xff);
            destination[offset + 2] = (byte) ((value >> 16) & 0xff);
            destination[offset + 3] = (byte) (value >> 24);
        }

        /// <summary>
        ///     Copes the short into the pre-allocated buffer
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="offset">The offset.</param>
        public static void GetBytes(short value, byte[] destination, int offset) {
            destination[offset] = (byte) (value & 0xff);
            destination[offset + 1] = (byte) ((value >> 8) & 0xff);
        }
    }
}
