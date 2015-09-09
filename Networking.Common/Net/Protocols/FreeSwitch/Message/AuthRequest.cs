using System.Collections.Specialized;

namespace Networking.Common.Net.Protocols.FreeSwitch.Message {
    /// <summary>
    ///     FreeSwitch auth/request Wrapper
    /// </summary>
    public class AuthRequest : EslMessage {
        /// <summary>
        ///     Authentication request
        /// </summary>
        public string Request { get { return Msg; } }

        public AuthRequest(string apiResponseMessage) : base(apiResponseMessage) { }
        public AuthRequest(NameValueCollection data, string msg) : base(data, msg) { }
    }
}