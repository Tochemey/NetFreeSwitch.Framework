﻿/*
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

namespace NetFreeSwitch.Framework.FreeSwitch.Commands {
    /// <summary>
    ///     Speak command
    ///     Speaks a string or file of text to the channel using the defined speech engine.
    /// </summary>
    public class SpeakCommand : BaseCommand {
        public SpeakCommand() {
            Engine = "flite";
            Voice = "kal";
        }

        /// <summary>
        ///     Engine
        /// </summary>
        public string Engine { set; get; }

        /// <summary>
        ///     Voice
        /// </summary>
        public string Voice { set; get; }

        /// <summary>
        ///     The actual to read
        /// </summary>
        public string Text { set; get; }

        /// <summary>
        ///     Timer
        /// </summary>
        public string TimerName { set; get; }

        public override string Command { get { return "speak"; } }

        public override string Argument { get { return Engine + "|" + Voice + "|" + Text + (!string.IsNullOrEmpty(TimerName) ? "|" + TimerName : ""); } }
    }
}