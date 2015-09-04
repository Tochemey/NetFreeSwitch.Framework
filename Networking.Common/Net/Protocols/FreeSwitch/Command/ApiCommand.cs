namespace Networking.Common.Net.Protocols.FreeSwitch.Command {
    /// <summary>
    ///     FreeSwitch API command.
    /// </summary>
    public class ApiCommand : BaseCommand {
        /// <summary>
        ///     The command string to send
        /// </summary>
        private readonly string _apiCommand;

        public ApiCommand(string apiCommand) { _apiCommand = apiCommand; }

        /// <summary>
        ///     The api command itself
        /// </summary>
        public override string Command { get { return "api"; } }

        /// <summary>
        ///     The api command argument
        /// </summary>
        public override string Argument { get { return _apiCommand; } }
    }
}