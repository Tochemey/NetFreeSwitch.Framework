using System.Collections.Generic;

namespace Networking.Common.Net.Protocols.FreeSwitch.Events {
    /// <summary>
    /// CALL_UPDATE FreeSwitch event wrapper
    /// </summary>
    public class CallUpdate : EslEvent {
        public CallUpdate(string message) : base(message) { }
        public CallUpdate(string message, string eventMessage) : base(message, eventMessage) { }

        public CallUpdate(Dictionary<string, string> parameters) { SetParameters(parameters); }

        public CallUpdate() { }
    }
}