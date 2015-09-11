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

namespace NetFreeSwitch.Framework.FreeSwitch.CallManager {
    /// <summary>
    ///     socket() application wrapper
    /// </summary>
    public class SocketApplication : OriginateApplication {
        private readonly List<string> _parameters;

        public SocketApplication(List<string> parameters) {
            if (parameters == null
                || !parameters.Any())
                throw new ArgumentNullException("parameters");
            _parameters = parameters;
        }

        public override string ApplicationName { get { return "socket"; } }
        public override IList<string> Arguments { get { return _parameters; } }
    }
}
