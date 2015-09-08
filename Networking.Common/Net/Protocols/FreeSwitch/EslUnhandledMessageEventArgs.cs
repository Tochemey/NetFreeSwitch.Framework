using System;

namespace Networking.Common.Net.Protocols.FreeSwitch {
    /// <summary>
    /// 
    /// </summary>
    public class EslUnhandledMessageEventArgs : EventArgs {

        private readonly EslMessage _message;
        public EslUnhandledMessageEventArgs(EslMessage message) { _message = message; }

        public EslMessage Message1 { get { return _message; } }
    }
}