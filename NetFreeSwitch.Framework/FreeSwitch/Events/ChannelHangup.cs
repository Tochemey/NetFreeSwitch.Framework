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

using System.Collections.Generic;
using System.Linq;

namespace NetFreeSwitch.Framework.FreeSwitch.Events {
    /// <summary>
    /// FreeSwitch CHANNEL_HANGUP event wrapper
    /// </summary>
    public class ChannelHangup : EslEvent {
        public ChannelHangup() { }
        public ChannelHangup(string message) : base(message) { }
        public ChannelHangup(string message, string eventMessage) : base(message, eventMessage) { }

        public ChannelHangup(Dictionary<string, string> parameters) { SetParameters(parameters); }

        public EslHangupCause Cause
        {
            get
            {
                if (Keys.Contains("Hangup-Cause")) {
                    string cause = this["Hangup-Cause"];
                    return Enumm.Parse<EslHangupCause>(cause);
                }
                return Enumm.Parse<EslHangupCause>("NONE");
            }
        }
    }
}
