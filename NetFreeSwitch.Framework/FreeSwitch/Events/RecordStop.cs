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
    public class RecordStop : EslEvent {
        public RecordStop(string message) : base(message) { }
        public RecordStop(string message, string eventMessage) : base(message, eventMessage) { }

        public RecordStop(Dictionary<string, string> parameters) { SetParameters(parameters); }

        public RecordStop() { }

        public string RecordFilePath { get { return this["Record-File-Path"]; } }

        public int RecordSeconds { get { return string.IsNullOrEmpty(this["record_seconds"]) ? 0 : this["record_seconds"].IsNumeric() ? Convert.ToInt32(this["record_seconds"]) : 0; } }

        public int RecordMilliSeconds { get { return string.IsNullOrEmpty(this["record_ms"]) ? 0 : this["record_ms"].IsNumeric() ? Convert.ToInt32(this["record_ms"]) : 0; } }
    }
}
