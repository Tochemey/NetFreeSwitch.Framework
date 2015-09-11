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
    ///     A object pool (or similar) have no more items to give out.
    /// </summary>
    /// <remarks>
    ///     This exception typically occurs if the pool/stack is too small (too many concurrent operations) or if some
    ///     code fails to return the item to the pool when done.
    /// </remarks>
    public class PoolEmptyException : Exception {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PoolEmptyException" /> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public PoolEmptyException(string errorMessage) : base(errorMessage) { }
    }
}
