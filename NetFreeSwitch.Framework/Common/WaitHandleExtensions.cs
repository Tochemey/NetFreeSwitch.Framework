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
using System.Threading;
using System.Threading.Tasks;

namespace NetFreeSwitch.Framework {
    /// <summary>
    ///     Extensions to make it easier to work with thread synchronization objects.
    /// </summary>
    public static class WaitHandleExtensions {
        /// <summary>
        ///     Convert a wait handle to a TPL Task.
        /// </summary>
        /// <param name="handle">Handle to convert</param>
        /// <returns>Generated task.</returns>
        //credits: http://stackoverflow.com/questions/18756354/wrapping-manualresetevent-as-awaitable-task
        public static Task AsTask(this WaitHandle handle) {
            return AsTask(handle, Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        ///     Convert a wait handle to a task
        /// </summary>
        /// <param name="handle">Wait handle</param>
        /// <param name="timeout">Max time to wait</param>
        /// <returns>Created task.</returns>
        // credits: http://stackoverflow.com/questions/18756354/wrapping-manualresetevent-as-awaitable-task
        public static Task AsTask(this WaitHandle handle, TimeSpan timeout) {
            var tcs = new TaskCompletionSource<object>();
            var registration = ThreadPool.RegisterWaitForSingleObject(handle,
                (state, timedOut) => {
                    var localTcs = (TaskCompletionSource<object>) state;
                    if (timedOut)
                        localTcs.TrySetCanceled();
                    else
                        localTcs.TrySetResult(null);
                },
                tcs,
                timeout,
                true);
            tcs.Task.ContinueWith((_, state) => ((RegisteredWaitHandle) state).Unregister(null), registration, TaskScheduler.Default);
            return tcs.Task;
        }
    }
}
