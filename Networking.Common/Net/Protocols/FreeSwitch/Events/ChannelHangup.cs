using System.Collections.Generic;
using System.Linq;

namespace Networking.Common.Net.Protocols.FreeSwitch.Events {
    /// <summary>
    /// FreeSwitch CHANNEL_HANGUP event wrapper
    /// </summary>
    public class ChannelHangup : EslEvent {
        public ChannelHangup() { }
        public ChannelHangup(string message) : base(message) { }
        public ChannelHangup(string message, string eventMessage) : base(message, eventMessage) { }

        public ChannelHangup(Dictionary<string, string> parameters) { SetParameters(parameters); }

        public EslHangupCause Cause
        {
            get
            {
                if (Keys.Contains("Hangup-Cause")) {
                    string cause = this["Hangup-Cause"];
                    return Enumm.Parse<EslHangupCause>(cause);
                }
                return Enumm.Parse<EslHangupCause>("NONE");
            }
        }
    }
}