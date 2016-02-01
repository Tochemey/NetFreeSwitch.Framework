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

using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace NetFreeSwitch.Framework.Common {
    public static class NetworkingExtensions {
        /// <summary>
        ///     Converts a string representing a host name or address to its <see cref="IPAddress" /> representation,
        ///     optionally opting to return a IpV6 address (defaults to IpV4)
        /// </summary>
        /// <param name="hostNameOrAddress">Host name or address to convert into an <see cref="IPAddress" /></param>
        /// <param name="favorIpV6">
        ///     When <code>true</code> will return an IpV6 address whenever available, otherwise
        ///     returns an IpV4 address instead.
        /// </param>
        /// <returns>
        ///     The <see cref="IPAddress" /> represented by <paramref name="hostNameOrAddress" /> in either IpV4 or
        ///     IpV6 (when available) format depending on <paramref name="favorIpV6" />
        /// </returns>
        public static IPAddress ToIPAddress(this string hostNameOrAddress, bool favorIpV6 = false) {
            var favoredFamily = favorIpV6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
            var addrs = Dns.GetHostAddresses(hostNameOrAddress);
            return addrs.FirstOrDefault(addr => addr.AddressFamily == favoredFamily)
                   ??
                   addrs.FirstOrDefault();
        }
    }
}