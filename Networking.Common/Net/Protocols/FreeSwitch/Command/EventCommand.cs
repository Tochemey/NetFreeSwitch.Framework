namespace Networking.Common.Net.Protocols.FreeSwitch.Command {
    /// <summary>
    /// Event command
    /// </summary>
    public class EventCommand : BaseCommand{
        /// <summary>
        /// Space separated list of events
        /// </summary>
        private readonly string _events;
        public EventCommand(string events) { _events = events; }
        public override string Command { get { return "event"; } }

        public override string Argument { get { return _events; } }
    }
}