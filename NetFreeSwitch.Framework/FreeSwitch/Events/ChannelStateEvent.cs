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
    public class ChannelStateEvent : EslEvent {
        public ChannelStateEvent(string message) : base(message) { }
        public ChannelStateEvent(string message, string eventMessage) : base(message, eventMessage) { }

        public ChannelStateEvent(Dictionary<string, string> parameters) { SetParameters(parameters); }

        public ChannelStateEvent() { }

        public string AnswerState { get { return this["Answer-State"]; } }

        public EslChannelDirection CallDirection { get { return Enumm.Parse<EslChannelDirection>(this["Call-Direction"]); } }

        public ChannelState ChannelState
        {
            get
            {
                string ch = this["Channel-State"];
                return string.IsNullOrEmpty(ch) ? ChannelState.UNKNOWN : Enumm.Parse<ChannelState>(ch.Trim());
            }
        }

        public CallState CallState
        {
            get
            {
                string cs = this["Channel-Call-State"];
                return string.IsNullOrEmpty(cs) ? CallState.DOWN : Enumm.Parse<CallState>(cs);
            }
        }

        public EslChannelDirection PresenceCallDirection
        {
            get
            {
                string pcd = this["Presence-Call-Direction"];
                return string.IsNullOrEmpty(pcd) ? EslChannelDirection.UNKNOWN : Enumm.Parse<EslChannelDirection>(this["Presence-Call-Direction"]);
            }
        }

        public bool HasHitDialplan { get { return Keys.Contains("Channel-HIT-Dialplan") && this["Channel-HIT-Dialplan"] == "true"; } }

        public string Protocol { get { return Keys.Contains("proto") ? this["proto"] : string.Empty; } }
    }
}
