using System.Collections.Specialized;

namespace Networking.Common.Net.Protocols.FreeSwitch.Message {
    /// <summary>
    ///     ApiResponse
    /// </summary>
    public class ApiResponse : EslMessage {
        /// <summary>
        ///     ApiResponse body
        /// </summary>
        public string Body { get { return OriginalMessage; } }

        public ApiResponse(string origMessage) : base(origMessage) { }
        public ApiResponse(NameValueCollection data, string origMessage) : base(data, origMessage) { }
    }
}