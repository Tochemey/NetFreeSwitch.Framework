namespace Networking.Common.Net.Protocols.Http.WebSocket {
    /// <summary>
    ///     WebSocket request includes the initial handshake request
    /// </summary>
    public class WebSocketRequest : WebSocketMessage {
        private readonly WebSocketFrame _frame;

        internal WebSocketRequest(IHttpRequest handshake, WebSocketFrame frame) : base(frame.Opcode, frame.Payload) {
            Handshake = handshake;
            _frame = frame;
        }

        /// <summary>
        ///     Cookies of the handshake request
        /// </summary>
        public IHttpRequest Handshake { get; }
    }
}