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

using System.Threading.Tasks;
using NetFreeSwitch.Framework.FreeSwitch.Commands;

namespace NetFreeSwitch.Framework.FreeSwitch {
    /// <summary>
    /// Used to send command
    /// </summary>
    public class CommandAsyncEvent {
        private readonly BaseCommand _command;
        private readonly TaskCompletionSource<EslMessage> _source;

        public CommandAsyncEvent(BaseCommand command)
        {
            _command = command;
            _source = new TaskCompletionSource<EslMessage>();
        }

        /// <summary>
        /// The FreeSwitch command to  send
        /// </summary>
        public BaseCommand Command { get { return _command; } }

        /// <summary>
        /// The response
        /// </summary>
        public Task<EslMessage> Task { get { return _source.Task; } }

        public void Complete(EslMessage response) { _source.TrySetResult(response); }

    }
}
