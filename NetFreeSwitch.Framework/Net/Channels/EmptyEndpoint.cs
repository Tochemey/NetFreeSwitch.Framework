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

using System.Net;
using System.Net.Sockets;

namespace NetFreeSwitch.Framework.Net.Channels {
    /// <summary>
    ///     Used when there are no connected endpoint for a channel
    /// </summary>
    public class EmptyEndpoint : EndPoint {
        /// <summary>
        ///     Instance representing an unassigned end point.
        /// </summary>
        public static readonly EmptyEndpoint Instance = new EmptyEndpoint();

        private EmptyEndpoint() { }

        /// <summary>
        ///     Gets the address family to which the endpoint belongs.
        /// </summary>
        /// <returns>
        ///     <c>AddressFamily.Unspecified</c>
        /// </returns>
        /// <exception cref="T:System.NotImplementedException">
        ///     Any attempt is made to get or set the property when the property is
        ///     not overridden in a descendant class.
        /// </exception>
        /// <PermissionSet>
        ///     <IPermission
        ///         class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Flags="UnmanagedCode, ControlEvidence" />
        /// </PermissionSet>
        public override AddressFamily AddressFamily { get { return AddressFamily.Unspecified; } }

        /// <summary>
        ///     Returns "None"
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString() { return "None"; }
    }
}
