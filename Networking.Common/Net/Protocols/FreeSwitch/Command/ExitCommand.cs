using System;

namespace Networking.Common.Net.Protocols.FreeSwitch.Command {
    public class ExitCommand : BaseCommand {
        public override string Command { get { return "exit"; } }

        public override string Argument { get { return String.Empty; } }
    }
}