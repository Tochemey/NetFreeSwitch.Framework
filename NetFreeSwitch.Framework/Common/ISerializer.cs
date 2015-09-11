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
using System.IO;
using System.Runtime.Serialization;

namespace NetFreeSwitch.Framework.Common
{
    /// <summary>
    ///     Serialize/deserialize an object
    /// </summary>
    public interface ISerializer {
        /// <summary>
        ///     Serialize
        /// </summary>
        /// <param name="source">object to serialize</param>
        /// <param name="destination">Stream to write to</param>
        /// <exception cref="SerializationException">Failed to serialize object</exception>
        void Serialize(object source, Stream destination);

        /// <summary>
        ///     Serialize
        /// </summary>
        /// <param name="source">object to serialize</param>
        /// <param name="destination">Stream to write to</param>
        /// <param name="baseType">
        ///     If specified, we should be able to differentiate sub classes, i.e. include type information if
        ///     <c>source</c> is of another type than this one.
        /// </param>
        /// <exception cref="SerializationException">Failed to serialize object</exception>
        void Serialize(object source, Stream destination, Type baseType);

        /// <summary>
        ///     Deserialize a stream
        /// </summary>
        /// <param name="source">Stream to read from</param>
        /// <param name="targetType">Type to deserialize. Should be the base type if inheritance is used.</param>
        /// <returns>Serialized object</returns>
        /// <exception cref="SerializationException">Failed to deserialize object</exception>
        object Deserialize(Stream source, Type targetType);
    }
}
