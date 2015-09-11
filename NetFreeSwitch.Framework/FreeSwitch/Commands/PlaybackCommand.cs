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
using System.Linq;

namespace NetFreeSwitch.Framework.FreeSwitch.Commands {
    /// <summary>
    ///     Playback wrapper
    /// </summary>
    public class PlaybackCommand : BaseCommand {
        private readonly string _audioFile;
        private readonly long _loop;
        private readonly IList<EslChannelVariable> _variables;

        public PlaybackCommand(string audioFile, IList<EslChannelVariable> variables, long loop) {
            if (string.IsNullOrEmpty(audioFile)) throw new ArgumentNullException("audioFile");
            _audioFile = audioFile;
            _variables = variables;
            _loop = loop;
        }

        public PlaybackCommand(string audioFile, IList<EslChannelVariable> variables) {
            if (string.IsNullOrEmpty(audioFile)) throw new ArgumentNullException("audioFile");
            _audioFile = audioFile;
            _variables = variables;
            _loop = 1;
        }

        public PlaybackCommand(string audioFile) {
            if (string.IsNullOrEmpty(audioFile)) throw new ArgumentNullException("audioFile");
            _audioFile = audioFile;
            _variables = new List<EslChannelVariable>();
            _loop = 1;
        }

        /// <summary>
        ///     Audio file to play
        /// </summary>
        public string AudioFile { get { return _audioFile; } }

        /// <summary>
        ///     The number of time to play the audio file. Please bear in mind that we will be using sendmsg to play audio file.
        ///     This one will be very helpful.
        /// </summary>
        public long Loop { get { return _loop; } }

        /// <summary>
        ///     Playback additional variables to add to the channel while playing the audio file
        /// </summary>
        public IList<EslChannelVariable> Variables { get { return _variables; } }

        public override string Command { get { return "playback"; } }

        public override string Argument { get { return ToString(); } }

        public override string ToString() {
            string variables = (_variables != null && _variables.Count > 0) ? _variables.Aggregate(string.Empty, (current, variable) => current + (variable + ",")) : string.Empty;
            if (variables.Length > 0) variables = "{" + variables.Remove(variables.Length - 1, 1) + "}";
            return string.Format("{0}{1}", variables, _audioFile);
        }
    }
}
