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

namespace NetFreeSwitch.Framework.FreeSwitch.Commands {
    /// <summary>
    ///     Helps to send an explicit hangup to a call with a specific reason.
    /// </summary>
    public class HangupCommand : BaseCommand {
        public const string CALL_COMMAND = "hangup";

        /// <summary>
        ///     The call id
        /// </summary>
        private readonly Guid _uuid;

        /// <summary>
        ///     The hangup cause
        /// </summary>
        private readonly string _reason;

        public HangupCommand(Guid uuid, string reason) {
            _uuid = uuid;
            _reason = reason;
        }

        public override string Command { get { return string.Format("sendmsg  {0}\ncall-command: {1}\nhangup-cause: {2}", _uuid, CALL_COMMAND, _reason); } }

        public override string Argument { get { return string.Empty; } }
    }
}
