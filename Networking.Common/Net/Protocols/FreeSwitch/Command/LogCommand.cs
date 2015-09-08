namespace Networking.Common.Net.Protocols.FreeSwitch.Command {
    /// <summary>
    ///     FreeSwitch log
    /// </summary>
    public class LogCommand : BaseCommand {
        /// <summary>
        /// Log level
        /// </summary>
        private readonly EslLogLevels _logLevel;
        public LogCommand(EslLogLevels logLevel) { _logLevel = logLevel; }
        public override string Command { get { return "log"; } }

        public override string Argument { get { return _logLevel.ToString(); } }
    }
}