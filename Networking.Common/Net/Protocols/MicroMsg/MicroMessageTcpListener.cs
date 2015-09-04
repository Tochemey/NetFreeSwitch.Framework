using Networking.Common.Net.Protocols.Serializers;

namespace Networking.Common.Net.Protocols.MicroMsg {
    /// <summary>
    ///     Server for the MicroMsg protocol
    /// </summary>
    public class MicroMessageTcpListener : ChannelTcpListener {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MicroMessageTcpListener" /> class.
        /// </summary>
        public MicroMessageTcpListener()
            : this(
                new ChannelTcpListenerConfiguration(() => (IMessageDecoder) new MicroMessageDecoder(new DataContractMessageSerializer()),
                    () => (IMessageEncoder) new MicroMessageEncoder(new DataContractMessageSerializer()))) {}

        /// <summary>
        ///     Initializes a new instance of the <see cref="MicroMessageTcpListener" /> class.
        /// </summary>
        /// <param name="configuration">Configuration to use.</param>
        public MicroMessageTcpListener(ChannelTcpListenerConfiguration configuration) : base(configuration) {}
    }
}