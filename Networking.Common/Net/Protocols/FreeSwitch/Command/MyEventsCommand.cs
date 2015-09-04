using System;

namespace Networking.Common.Net.Protocols.FreeSwitch.Command {
    /// <summary>
    ///     Helps to listen a specific channel events.
    /// </summary>
    public class MyEventsCommand : BaseCommand {
        /// <summary>
        ///     Channel Id
        /// </summary>
        private readonly Guid _uuid;

        public MyEventsCommand(Guid uuid) { _uuid = uuid; }
        public override string Command { get { return "myevents"; } }

        public override string Argument { get { return _uuid.ToString(); } }
    }
}