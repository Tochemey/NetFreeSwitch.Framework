using System.Collections.Generic;

namespace Networking.Common.Net.Protocols.FreeSwitch.Events {
    public class SessionHeartbeat : EslEvent {
        public SessionHeartbeat(string message) : base(message) { }
        public SessionHeartbeat(string message, string eventMessage) : base(message, eventMessage) { }

        public SessionHeartbeat(Dictionary<string, string> parameters) { SetParameters(parameters); }

        public SessionHeartbeat() { }
    }
}