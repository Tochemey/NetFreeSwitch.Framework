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
    ///     sleep.
    ///     Pause the channel for a given number of milliseconds, consuming the audio for that period of time.
    ///     Calling sleep also will consume any outstanding RTP on the operating system's input queue, which can be very useful
    ///     in situations where audio becomes backlogged.
    /// </summary>
    public class SleepCommand : BaseCommand {
        public SleepCommand(long duration) {
            Duration = duration;
        }

        /// <summary>
        /// How long to pause the channel
        /// </summary>
        public long Duration { private set; get; }
        public override string Command { get { return "sleep"; } }
        public override string Argument { get { return Duration.ToString(); } }
    }
}