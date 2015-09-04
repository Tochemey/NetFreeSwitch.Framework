using Networking.Common.Net.Protocols.MicroMsg;
using Networking.Common.Net.Protocols.Serializers;

namespace Networking.Common.Net.Protocols.Stomp {
    /// <summary>
    ///     Used to talk with a STOMP server.
    /// </summary>
    /// <remarks>
    ///     This client can only parse STOMP frames, it doesn't know what they mean.
    /// </remarks>
    public class StompClientLight : ChannelTcpClient {
        /// <summary>
        ///     Initializes a new instance of the <see cref="StompClientLight" /> class.
        /// </summary>
        /// <param name="serializer">The serializer used to encode/decode body.</param>
        public StompClientLight(IMessageSerializer serializer) : base(new MicroMessageEncoder(serializer), new MicroMessageDecoder(serializer)) { }
    }
}