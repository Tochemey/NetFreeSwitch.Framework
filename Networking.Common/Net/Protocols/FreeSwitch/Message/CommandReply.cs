﻿using System.Collections.Specialized;

namespace Networking.Common.Net.Protocols.FreeSwitch.Message {
    /// <summary>
    ///     FreeSwitch CommandReply
    /// </summary>
    public class CommandReply : EslMessage {
        /// <summary>
        ///     Constructor to set the original message
        /// </summary>
        /// <param name="origMessage">Original message</param>
        public CommandReply(string origMessage) : base(origMessage) { }

        /// <summary>
        ///     Constructor to set the decoded message and the original message
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="origMessage">Original message</param>
        public CommandReply(NameValueCollection data, string origMessage) : base(data, origMessage) { }

        /// <summary>
        ///     Command Reply Text
        /// </summary>
        public string Reply { get { return this["Reply-Text"]; } }

        /// <summary>
        ///     Check whether the CommandReply was a successful reply or not.
        /// </summary>
        public bool IsSuccessful { get { return Reply.StartsWith("+OK"); } }
    }
}