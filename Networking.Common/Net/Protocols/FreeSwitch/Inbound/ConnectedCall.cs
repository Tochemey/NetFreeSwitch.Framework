using System;
using System.Collections.Generic;

namespace Networking.Common.Net.Protocols.FreeSwitch.Inbound {
    /// <summary>
    ///     Wrapper of Channel Data event.
    /// </summary>
    public class ConnectedCall {
        private readonly Dictionary<string, string> _parameters;

        public ConnectedCall(Dictionary<string, string> parameters) { _parameters = parameters; }

        public ConnectedCall() { }

        public string CallerIdNumber { get { return this["Caller-Caller-ID-Number"]; } }

        public string CallerUniqueID { get { return this["Caller-Unique-ID"]; } }

        public string ChannelName { get { return this["Channel-Name"]; } }

        public string DestinationNumber { get { return this["Channel-Destination-Number"]; } }
        public string DomainName { get { return this["domain_name"]; } }
        public string UniqueID { get { return this["Unique-ID"]; } }
        public string UserContext { get { return this["user_context"]; } }

        public string this[string name]
        {
            get
            {
                if (_parameters.ContainsKey(name))
                    return Uri.UnescapeDataString(_parameters[name]);
                return _parameters.ContainsKey("variable_" + name) ? Uri.UnescapeDataString(_parameters["variable_" + name]) : null;
            }
        }
    }
}