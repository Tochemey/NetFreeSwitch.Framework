using System;
using Networking.Common.Net.Protocols.FreeSwitch.Message;

namespace Networking.Common.Net.Protocols.FreeSwitch {
    public class EslDisconnectNoticeEventArgs : EventArgs {
        private readonly DisconnectNotice _notice;
        public EslDisconnectNoticeEventArgs(DisconnectNotice notice) { _notice = notice; }

        public DisconnectNotice Notice { get { return _notice; } }
    }
}