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
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace NetFreeSwitch.Framework.Net.Channels {
    /// <summary>
    ///     Client X.509 certificate, X.509 chain, and any SSL policy errors encountered
    ///     during the SSL stream creation
    /// </summary>
    public class ClientCertificate {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ClientCertificate" /> class.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        /// <param name="chain">Client security certificate chain.</param>
        /// <param name="sslPolicyErrors">Any SSL policy errors encountered during the SSL stream creation.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     certificate
        ///     or
        ///     chain
        /// </exception>
        public ClientCertificate(X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            if (certificate == null) throw new ArgumentNullException("certificate");
            if (chain == null) throw new ArgumentNullException("chain");

            Certificate = certificate;
            Chain = chain;
            SslPolicyErrors = sslPolicyErrors;
        }

        /// <summary>
        ///     Client security certificate
        /// </summary>
        public X509Certificate Certificate { get; private set; }

        /// <summary>
        ///     Client security certificate chain
        /// </summary>
        public X509Chain Chain { get; private set; }

        /// <summary>
        ///     Any SSL policy errors encountered during the SSL stream creation
        /// </summary>
        public SslPolicyErrors SslPolicyErrors { get; private set; }
    }
}
