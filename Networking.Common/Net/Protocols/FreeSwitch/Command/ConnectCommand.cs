namespace Networking.Common.Net.Protocols.FreeSwitch.Command {
    /// <summary>
    ///     Used to connect to FreeSwitch.
    /// </summary>
    public class ConnectCommand : BaseCommand {
        public override string Command { get { return "connect"; } }

        public override string Argument { get { return string.Empty; } }
    }
}