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

namespace NetFreeSwitch.Framework.FreeSwitch.Commands {
    /// <summary>
    ///     SetVar helps set a variable for a specific FreeSwitch channel using the ApiCommand
    /// </summary>
    public class SetVarCommand : BaseCommand {
        /// <summary>
        ///     Channel Id
        /// </summary>
        private readonly string _uuid;

        /// <summary>
        ///     Variable name
        /// </summary>
        private readonly string _name;

        /// <summary>
        ///     Variable value
        /// </summary>
        private readonly string _value;

        public SetVarCommand(string uuid, string name, string value) {
            _uuid = uuid;
            _name = name;
            _value = value;
        }

        public override string Command { get { return "uuid_setvar"; } }

        public override string Argument { get { return string.Format("{0} {1} {2}", _uuid, Name, Value); } }

        /// <summary>
        ///     Variable name
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        ///     Variable value
        /// </summary>
        public string Value { get { return _value; } }
    }
}
