using System.Collections.Generic;

namespace Networking.Common.Net.Protocols.FreeSwitch.Events {
    /// <summary>
    ///    CHANNEL_PROGRESS_MEDIA event wrapper
    /// </summary>
    public class ChannelProgressMedia : EslEvent {
        public ChannelProgressMedia(string message) : base(message) { }
        public ChannelProgressMedia(string message, string eventMessage) : base(message, eventMessage) { }

        public ChannelProgressMedia(Dictionary<string, string> parameters) { SetParameters(parameters); }

        public ChannelProgressMedia() { }
    }
}