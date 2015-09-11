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
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace NetFreeSwitch.Framework.Net.Channels {
    /// <summary>
    ///     Builder used to create SslStreams for client side applications.
    /// </summary>
    public class ClientSideSslStreamBuilder : ISslStreamBuilder {
        /// <summary>
        /// </summary>
        /// <param name="commonName">the domain name of the server that you are connecting to</param>
        public ClientSideSslStreamBuilder(string commonName) {
            CommonName = commonName;
            Protocols = SslProtocols.Default;
        }

        /// <summary>
        ///     Typically the domain name of the server that you are connecting to.
        /// </summary>
        public string CommonName { get; }

        /// <summary>
        ///     Leave empty to use the server certificate
        /// </summary>
        public X509Certificate Certificate { get; set; }

        /// <summary>
        ///     Allowed SSL protocols
        /// </summary>
        public SslProtocols Protocols { get; set; }

        /// <summary>
        ///     Build a new SSL steam.
        /// </summary>
        /// <param name="channel">Channel which will use the stream</param>
        /// <param name="socket">Socket to wrap</param>
        /// <returns>Stream which is ready to be used (must have been validated)</returns>
        public SslStream Build(ITcpChannel channel, Socket socket) {
            var ns = new NetworkStream(socket);
            var stream = new SslStream(ns, true, OnRemoteCertifiateValidation);

            try {
                X509CertificateCollection certificates = null;
                if (Certificate != null) certificates = new X509CertificateCollection(new[] {Certificate});

                stream.AuthenticateAsClient(CommonName, certificates, Protocols, false);
            }
            catch (IOException err) {
                throw new InvalidOperationException("Failed to authenticate " + socket.RemoteEndPoint, err);
            }
            catch (ObjectDisposedException err) {
                throw new InvalidOperationException("Failed to create stream, did client disconnect directly?", err);
            }
            catch (AuthenticationException err) {
                throw new InvalidOperationException("Failed to authenticate " + socket.RemoteEndPoint, err);
            }

            return stream;
        }

        /// <summary>
        ///     Used to validate the certificate that the server have provided.
        /// </summary>
        /// <param name="sender">Server.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="chain">The chain.</param>
        /// <param name="sslpolicyerrors">The sslpolicyerrors.</param>
        /// <returns><c>true</c> if the certificate will be allowed, otherwise <c>false</c>.</returns>
        protected virtual bool OnRemoteCertifiateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors) {
            return (Certificate != null && certificate == null) || (Certificate == null && certificate != null);
        }
    }
}
