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

using System.Collections.Specialized;

namespace NetFreeSwitch.Framework.FreeSwitch.Messages {
    /// <summary>
    ///     FreeSwitch CommandReply
    /// </summary>
    public class CommandReply : EslMessage {
        /// <summary>
        ///     Constructor to set the original message
        /// </summary>
        /// <param name="apiResponseMessage">Original message</param>
        public CommandReply(string apiResponseMessage) : base(apiResponseMessage) { }

        /// <summary>
        ///     Constructor to set the decoded message and the original message
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="msg">Original message</param>
        public CommandReply(NameValueCollection data, string msg) : base(data, msg) { }

        /// <summary>
        ///     Command Reply Text
        /// </summary>
        public string Reply { get { return this["Reply-Text"]; } }

        /// <summary>
        ///     Check whether the CommandReply was a successful reply or not.
        /// </summary>
        public bool IsSuccessful { get { return Reply.StartsWith("+OK"); } }
    }
}
