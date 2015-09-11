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
using System.Collections.Generic;

namespace NetFreeSwitch.Framework.FreeSwitch.Events {
    /// <summary>
    ///     FreeSwitch Event wrapper
    /// </summary>
    public class EslEvent : EslMessage {
        public EslEvent(string message) : this(message, null) { }

        public EslEvent(string message, string eventMessage) : base(message) { Message = eventMessage; }

        public EslEvent() { }

        public EslEvent(Dictionary<string, string> parameters) { SetParameters(parameters); }

        public string EventName { get { return this["Event-Name"]; } }

        public string EventSubClass { get { return this["Event-Subclass"]; } }

        public string CoreUuid { get { return this["Core-UUID"]; } }

        public DateTime EventDateLocal { get { return DateTime.Parse(this["Event-Date-Local"]); } }

        public DateTime EventDateGmt { get { return DateTime.Parse(this["Event-Date-GMT"]); } }

        public DateTime EventTimeStamp { get { return this["Event-Date-timestamp"].FromUnixTime(); } }

        public string EventCallingFile { get { return this["Event-Calling-File"]; } }

        public string EventCallingFunction { get { return this["Event-Calling-Function"]; } }

        public string EventCallingLineNumber { get { return this["Event-Calling-Line-Number"]; } }

        public string UniqueId { get { return this["Unique-ID"]; } }

        public string CallerUuid { get { return this["Caller-Unique-ID"]; } }

        public string ChannelName { get { return this["Channel-Name"]; } }

        public string ValidatingPin { get { return this["variable_inputted_validation_pin"]; } }

        public string Message { get; set; }

        public EslEvent AddHeader(string header, string val) {
            Items.Add(header, val);
            return this;
        }
    }
}
