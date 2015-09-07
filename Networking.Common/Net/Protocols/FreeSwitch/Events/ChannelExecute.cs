using System.Collections.Generic;

namespace Networking.Common.Net.Protocols.FreeSwitch.Events {
    /// <summary>
    ///     FreeSwitch CHANNEL_EXECUTE event wrapper
    /// </summary>
    public class ChannelExecute : EslEvent {
        public ChannelExecute(string message) : base(message) { }

        public ChannelExecute(string message, string eventMessage) : base(message, eventMessage) { }

        public ChannelExecute() { }

        public ChannelExecute(Dictionary<string, string> parameters) { SetParameters(parameters); }

        /// <summary>
        ///     Application name
        /// </summary>
        public string Application { get { return this["Application"]; } }

        /// <summary>
        ///     Application data
        /// </summary>
        public string ApplicationData { get { return this["Application-Data"]; } }

        /// <summary>
        ///     Application Response
        /// </summary>
        protected string ApplicationResponse { get { return this["Application-Response"]; } }

        public override string ToString() { return "ChannelExecute(" + Application + ", '" + ApplicationData + "')"; }
    }
}