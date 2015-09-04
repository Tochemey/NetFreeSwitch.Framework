using System;

namespace Networking.Common.Net.Protocols.Stomp.Broker.Services {
    /// <summary>
    ///     Simply says that authentication is not activated (i.e. should be skipped)
    /// </summary>
    internal class NoAuthenticationService : IAuthenticationService {
        public bool IsActivated { get { return false; } }

        public LoginResponse Login(string user, string passcode) { throw new NotSupportedException(); }
    }
}