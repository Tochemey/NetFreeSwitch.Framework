using System.Collections.Generic;

namespace Networking.Common.Net.Protocols.FreeSwitch.Events {
    public class Custom : EslEvent {
        public Custom() { }
        public Custom(string message) : base(message) { }
        public Custom(string message, string eventMessage) : base(message, eventMessage) { }

        public Custom(Dictionary<string, string> parameters) { SetParameters(parameters); }
    }
}