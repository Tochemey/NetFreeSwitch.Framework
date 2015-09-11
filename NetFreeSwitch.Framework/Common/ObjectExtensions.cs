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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetFreeSwitch.Framework.Common {
    /// <summary>
    ///     Extension methods for object.
    /// </summary>
    public static class ObjectExtensions {
        /// <summary>
        ///     Turn anonymous object to dictionary
        /// </summary>
        /// <param name="data">Anonymous object</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<string, object> ToDictionary(this object data) {
            return (from property in data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance) where property.CanRead select property).ToDictionary(property => property.Name,
                property => property.GetValue(data, null));
        }
    }
}
