using System.Collections.Generic;

namespace Networking.Common.Net.Protocols.FreeSwitch.Events {
    /// <summary>
    /// FreeSwitch CHANNEL_PARK event wrapper
    /// </summary>
    public class ChannelPark : EslEvent {
        public ChannelPark(string message) : base(message) { }
        public ChannelPark(string message, string eventMessage) : base(message, eventMessage) { }
        public ChannelPark() { }

        public ChannelPark(Dictionary<string, string> parameters) { SetParameters(parameters); }
    }
}