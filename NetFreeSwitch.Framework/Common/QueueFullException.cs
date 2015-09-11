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

namespace NetFreeSwitch.Framework.Common {
    /// <summary>
    ///     Queue is full and no more items may be enqueued.
    /// </summary>
    [Serializable]
    public class QueueFullException : Exception {
        /// <summary>
        ///     Initializes a new instance of the <see cref="QueueFullException" /> class.
        /// </summary>
        /// <param name="queueName">Name of the queue.</param>
        public QueueFullException(string queueName) : base("Queue '" + queueName + "' is full.") { QueueName = queueName; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueueFullException" /> class.
        /// </summary>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="inner">The inner.</param>
        public QueueFullException(string queueName, Exception inner) : base("Queue '" + queueName + "' is full.", inner) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueueFullException" /> class.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
        ///     data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
        ///     information about the source or destination.
        /// </param>
        protected QueueFullException(SerializationInfo info, StreamingContext context) : base(info, context) { QueueName = info.GetString("QueueName"); }

        /// <summary>
        ///     Name of the queue that is full.
        /// </summary>
        public string QueueName { get; }

        /// <summary>
        ///     When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with
        ///     information about the exception.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
        ///     data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
        ///     information about the source or destination.
        /// </param>
        /// <PermissionSet>
        ///     <IPermission
        ///         class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
        ///     <IPermission
        ///         class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Flags="SerializationFormatter" />
        /// </PermissionSet>
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("QueueName", QueueName);
            base.GetObjectData(info, context);
        }
    }
}
