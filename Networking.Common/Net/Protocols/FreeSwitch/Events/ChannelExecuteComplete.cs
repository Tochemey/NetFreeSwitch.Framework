using System.Collections.Generic;

namespace Networking.Common.Net.Protocols.FreeSwitch.Events {
    /// <summary>
    /// FreeSwitch CHANNEL_EXECUTE_COMPLETE event wrapper
    /// </summary>
    public class ChannelExecuteComplete : EslEvent {
        public ChannelExecuteComplete(string message) : base(message) { }
        public ChannelExecuteComplete(string message, string eventMessage) : base(message, eventMessage) { }
        public ChannelExecuteComplete() { }

        public ChannelExecuteComplete(Dictionary<string, string> parameters) { SetParameters(parameters); }

        /// <summary>
        /// Application name
        /// </summary>
        public string Application { get { return this["Application"]; } }

        /// <summary>
        /// Application Data
        /// </summary>
        public string ApplicationData { get { return this["Application-Data"]; } }

        /// <summary>
        ///     Application Response
        /// </summary>
        public string ApplicationResponse { get { return this["Application-Response"]; } }

        public override string ToString() { return "ExecuteComplete(" + Application + ", '" + ApplicationData + "')." + base.ToString(); }
    }
}