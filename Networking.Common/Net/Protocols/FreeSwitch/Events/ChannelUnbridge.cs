using System.Collections.Generic;

namespace Networking.Common.Net.Protocols.FreeSwitch.Events {
    public class ChannelUnbridge : EslEvent {
        public ChannelUnbridge(string message) : base(message) { }
        public ChannelUnbridge(string message, string eventMessage) : base(message, eventMessage) { }

        public ChannelUnbridge(Dictionary<string, string> parameters) { SetParameters(parameters); }

        public ChannelUnbridge() { }
    }
}