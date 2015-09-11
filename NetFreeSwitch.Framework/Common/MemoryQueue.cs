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

using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NetFreeSwitch.Framework.Common {
    /// <summary>
    ///     Wraps <c><![CDATA[ConcurrentQueue<T>]]></c>
    /// </summary>
    /// <typeparam name="T">Type of item to store.</typeparam>
    public class MemoryQueue<T> : IQueue<T> {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        /// <summary>
        ///     Dequeue an item from our queue.
        /// </summary>
        /// <returns>Dequeued item; <c>default(T)</c> if there are no more items in the queue.</returns>
        public Task<T> DequeueAsync() {
            T item;
            return _queue.TryDequeue(out item) ? Task.FromResult(item) : Task.FromResult(default(T));
        }

        /// <summary>
        ///     Enqueue item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
#pragma warning disable 1998
        public async Task EnqueueAsync(T item)
#pragma warning restore 1998
        { _queue.Enqueue(item); }
    }
}
