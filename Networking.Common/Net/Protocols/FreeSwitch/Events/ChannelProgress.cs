using System.Collections.Generic;

namespace Networking.Common.Net.Protocols.FreeSwitch.Events {
    /// <summary>
    ///     FreeSwitch CHANNEL_PROGRESS event wrapper
    /// </summary>
    public class ChannelProgress : ChannelStateEvent {
        public ChannelProgress(string message) : base(message) { }
        public ChannelProgress(string message, string eventMessage) : base(message, eventMessage) { }

        public ChannelProgress() { }

        public ChannelProgress(Dictionary<string, string> parameters) { SetParameters(parameters); }
    }
}