using System.Collections.Specialized;

namespace Networking.Common.Net.Protocols.FreeSwitch.Message {
    /// <summary>
    ///     FreeSwitch auth/request Wrapper
    /// </summary>
    public class AuthRequest : EslMessage {
        /// <summary>
        ///     Authentication request
        /// </summary>
        public string Request { get { return OriginalMessage; } }

        public AuthRequest(string origMessage) : base(origMessage) { }
        public AuthRequest(NameValueCollection data, string origMessage) : base(data, origMessage) { }
    }
}