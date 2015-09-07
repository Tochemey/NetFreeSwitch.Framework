using System;
using Networking.Common.Net.Protocols.FreeSwitch.Events;

namespace Networking.Common.Net.Protocols.FreeSwitch {
    public class EslEventArgs : EventArgs
    {
        private readonly EslEvent _eventReceived;
        public EslEventArgs(EslEvent eventReceived) { _eventReceived = eventReceived; }

        public EslEvent EventReceived { get { return _eventReceived; } }
    }
}