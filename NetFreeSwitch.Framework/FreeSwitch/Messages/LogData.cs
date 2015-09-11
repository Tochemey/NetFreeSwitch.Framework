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
using System.Collections.Specialized;

namespace NetFreeSwitch.Framework.FreeSwitch.Messages {
    /// <summary>
    ///     FreeSwitch log/data wrapper
    /// </summary>
    public class LogData : EslMessage {
        /// <summary>
        ///     ApiResponse body
        /// </summary>
        public string Body { get { return Msg; } }

        public LogData(string message) : base(message) { }
        public LogData(NameValueCollection data, string msg) : base(data, msg) { }

        public EslLogLevels? Level
        {
            get
            {
                if (this["Log-Level"] == null)
                    return null;
                int num = int.Parse(this["Log-Level"]);
                if (num > (int) EslLogLevels.DEBUG)
                    return EslLogLevels.DEBUG;
                return (EslLogLevels) num;
            }
        }

        public string TextChannel { get { return this["Text-Channel"]; } }

        public string LogFile { get { return this["Log-File"]; } }

        public string LogFunc { get { return this["Log-Func"]; } }

        public string LogLine { get { return this["Log-Line"]; } }

        public string UserData { get { return this["User-Data"]; } }

        public string ContentLength { get { return this["Content-Length"]; } }

        public DateTime MsgTime { get { return DateTime.Parse(Body.Substring(0, Body.IndexOf("[", StringComparison.Ordinal)).Trim()); } }

        public string LogLineText { get { return Body.Substring(Body.IndexOf(LogFile + ":" + LogLine, StringComparison.Ordinal) + (LogFile + ":" + LogLine).Length); } }
    }
}
