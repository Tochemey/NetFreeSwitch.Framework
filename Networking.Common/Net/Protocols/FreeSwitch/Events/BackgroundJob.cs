using System.Collections.Generic;

namespace Networking.Common.Net.Protocols.FreeSwitch.Events {
    /// <summary>
    ///     BACKGROUND_JOB event wrapper
    /// </summary>
    public class BackgroundJob : EslEvent {
        public BackgroundJob(string message) : base(message) { }

        public BackgroundJob(string message, string eventMessage) : base(message, eventMessage) { }

        public BackgroundJob() { }

        public BackgroundJob(Dictionary<string, string> parameters) { SetParameters(parameters); }

        /// <summary>
        ///     Gets ID of the bgapi job
        /// </summary>
        public string JobUid { get { return this["Job-UUID"]; } }

        /// <summary>
        ///     Gets command which was executed
        /// </summary>
        public string CommandName { get { return this["Job-Command"]; } }

        /// <summary>
        ///     Gets arguments for the command
        /// </summary>
        public string CommandArguments { get { return this["Job-Command-Arg"]; } }

        /// <summary>
        ///     Gets the actual command result.
        /// </summary>
        public string CommandResult
        {
            get
            {
                string commandResult = this["__CONTENT__"];
                if (!string.IsNullOrEmpty(commandResult)) return commandResult;
                foreach (string key in Items.Keys) {
                    if (key.StartsWith("+OK")
                        || key.StartsWith("-ERR")) {
                        commandResult = key + ":" + this[key];
                        break;
                    }
                }
                return commandResult;
            }
        }

        /// <summary>
        ///     Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() { return CommandName + "(" + CommandArguments + ") = '" + CommandResult + "'\r\n\t"; }
    }
}