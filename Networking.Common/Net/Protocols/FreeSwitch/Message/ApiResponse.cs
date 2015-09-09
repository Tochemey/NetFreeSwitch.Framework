using System.Collections.Specialized;

namespace Networking.Common.Net.Protocols.FreeSwitch.Message {
    /// <summary>
    ///     ApiResponse
    /// </summary>
    public class ApiResponse : EslMessage {
        /// <summary>
        ///     ApiResponse body
        /// </summary>
        public string Body { get { return Msg; } }

        public ApiResponse(string message) : base(message) { }
        public ApiResponse(NameValueCollection data, string msg) : base(data, msg) { }
    }
}