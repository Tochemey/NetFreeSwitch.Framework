using System;
using System.Collections.Generic;

namespace Networking.Common.Net.Protocols.FreeSwitch.Events {
    public class Dtmf : EslEvent {
        public Dtmf(string message) : base(message) { }
        public Dtmf(string message, string eventMessage) : base(message, eventMessage) { }

        public Dtmf() { }

        public Dtmf(Dictionary<string, string> parameters) { SetParameters(parameters); }

        public char Digit { get { return Convert.ToChar(this["DTMF-Digit"]); } }

        public int Duration
        {
            get
            {
                int duration;
                return int.TryParse(this["DTMF-Duration"], out duration) ? duration : 0;
            }
        }

        public override string ToString() { return "Dtmf(" + Digit + ")."; }
    }
}