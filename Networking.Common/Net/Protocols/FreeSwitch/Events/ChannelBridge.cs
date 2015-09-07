using System.Collections.Generic;

namespace Networking.Common.Net.Protocols.FreeSwitch.Events {
    /// <summary>
    /// FreeSwitch CHANNEL_BRIDGE event wrapper
    /// </summary>
    public class ChannelBridge : EslEvent {
        public ChannelBridge(string message) : base(message) { }
        public ChannelBridge(string message, string eventMessage) : base(message, eventMessage) { }

        public ChannelBridge() { }

        public ChannelBridge(Dictionary<string, string> parameters) { SetParameters(parameters); }
    }
}