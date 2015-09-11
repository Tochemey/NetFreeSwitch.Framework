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

namespace NetFreeSwitch.Framework.FreeSwitch.Inbound {
    /// <summary>
    ///     Wrapper of Channel Data event.
    /// </summary>
    public class ConnectedCall {
        private readonly Dictionary<string, string> _parameters;

        public ConnectedCall(Dictionary<string, string> parameters) { _parameters = parameters; }

        public ConnectedCall() { }

        public string CallerIdNumber { get { return this["Caller-Caller-ID-Number"]; } }

        public string CallerUniqueID { get { return this["Caller-Unique-ID"]; } }

        public string ChannelName { get { return this["Channel-Name"]; } }

        public string DestinationNumber { get { return this["Channel-Destination-Number"]; } }
        public string DomainName { get { return this["domain_name"]; } }
        public string UniqueID { get { return this["Unique-ID"]; } }
        public string UserContext { get { return this["user_context"]; } }

        public string this[string name]
        {
            get
            {
                if (_parameters.ContainsKey(name))
                    return Uri.UnescapeDataString(_parameters[name]);
                return _parameters.ContainsKey("variable_" + name) ? Uri.UnescapeDataString(_parameters["variable_" + name]) : null;
            }
        }
    }
}
