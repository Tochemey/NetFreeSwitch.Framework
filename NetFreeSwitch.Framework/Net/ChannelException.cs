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
using System.Runtime.Serialization;

namespace NetFreeSwitch.Framework.Net {
    /// <summary>
    ///     Channel failed to work as expected.
    /// </summary>
    [Serializable]
    public class ChannelException : Exception {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ChannelException" /> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="inner">The inner.</param>
        public ChannelException(string errorMessage, Exception inner) : base(errorMessage, inner) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChannelException" /> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public ChannelException(string errorMessage) : base(errorMessage) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChannelException" /> class.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
        ///     data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
        ///     information about the source or destination.
        /// </param>
        protected ChannelException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
