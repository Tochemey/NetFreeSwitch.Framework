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

namespace NetFreeSwitch.Framework.Net.Channels {
    /// <summary>
    ///     Uses a concurrent dictionary to store all items.
    /// </summary>
    public class ChannelData : IChannelData {
        private readonly ConcurrentDictionary<string, object> _items = new ConcurrentDictionary<string, object>();

        /// <summary>
        ///     Get or add a value
        /// </summary>
        /// <param name="key">key to get</param>
        /// <param name="addCallback">Should return value to add if the key is not found</param>
        /// <returns>Item that was added or found</returns>
        public object GetOrAdd(string key, Func<string, object> addCallback) { return _items.GetOrAdd(key, addCallback); }

        /// <summary>
        ///     Try updating a value
        /// </summary>
        /// <param name="key">Key for the value to update</param>
        /// <param name="newValue">Value to set</param>
        /// <param name="existingValue">Value that we've previously retrieved</param>
        /// <returns><c>true</c> if the existing value is the same as the one in the dictionary</returns>
        public bool TryUpdate(string key, object newValue, object existingValue) { return _items.TryUpdate(key, newValue, existingValue); }

        /// <summary>
        ///     Get or set data
        /// </summary>
        /// <param name="key">Identifier (note that everyone with access to the channel can access the data, use careful naming)</param>
        /// <returns>Data if found; otherwise <c>null</c>.</returns>
        public object this[string key]
        {
            get
            {
                object item;
                return _items.TryGetValue(key, out item) ? item : null;
            }
            set { _items[key] = value; }
        }

        /// <summary>
        ///     Remove all existing data.
        /// </summary>
        public void Clear() { _items.Clear(); }
    }
}
