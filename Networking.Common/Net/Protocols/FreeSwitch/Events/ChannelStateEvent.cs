using System.Collections.Generic;
using System.Linq;

namespace Networking.Common.Net.Protocols.FreeSwitch.Events {
    public class ChannelStateEvent : EslEvent {
        public ChannelStateEvent(string message) : base(message) { }
        public ChannelStateEvent(string message, string eventMessage) : base(message, eventMessage) { }

        public ChannelStateEvent(Dictionary<string, string> parameters) { SetParameters(parameters); }

        public ChannelStateEvent() { }

        public string AnswerState { get { return this["Answer-State"]; } }

        public EslChannelDirection CallDirection { get { return Enumm.Parse<EslChannelDirection>(this["Call-Direction"]); } }

        public ChannelState ChannelState
        {
            get
            {
                string ch = this["Channel-State"];
                return string.IsNullOrEmpty(ch) ? ChannelState.UNKNOWN : Enumm.Parse<ChannelState>(ch.Trim());
            }
        }

        public CallState CallState
        {
            get
            {
                string cs = this["Channel-Call-State"];
                return string.IsNullOrEmpty(cs) ? CallState.DOWN : Enumm.Parse<CallState>(cs);
            }
        }

        public EslChannelDirection PresenceCallDirection
        {
            get
            {
                string pcd = this["Presence-Call-Direction"];
                return string.IsNullOrEmpty(pcd) ? EslChannelDirection.UNKNOWN : Enumm.Parse<EslChannelDirection>(this["Presence-Call-Direction"]);
            }
        }

        public bool HasHitDialplan { get { return Keys.Contains("Channel-HIT-Dialplan") && this["Channel-HIT-Dialplan"] == "true"; } }

        public string Protocol { get { return Keys.Contains("proto") ? this["proto"] : string.Empty; } }
    }
}