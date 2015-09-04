using System;

namespace Networking.Common.Net.Protocols.FreeSwitch.Command {
    /// <summary>
    ///     Resume command
    /// </summary>
    public class ResumeCommand : BaseCommand {
        public override string Command { get { return "resume"; } }

        public override string Argument { get { return String.Empty; } }
    }
}