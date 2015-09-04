namespace Networking.Common.Net.Protocols.FreeSwitch.Command {
    /// <summary>
    ///     Used to disable an event on FreeSwitch
    /// </summary>
    public class NixEventCommand : BaseCommand {
        /// <summary>
        ///     Event Name
        /// </summary>
        private readonly string _eventName;

        public NixEventCommand(string eventName) { _eventName = eventName; }
        public override string Command { get { return "nixevent"; } }

        public override string Argument { get { return _eventName; } }
    }
}