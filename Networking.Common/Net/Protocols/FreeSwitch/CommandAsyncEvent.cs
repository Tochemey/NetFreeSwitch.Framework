using System.Threading.Tasks;
using Networking.Common.Net.Protocols.FreeSwitch.Command;

namespace Networking.Common.Net.Protocols.FreeSwitch {
    /// <summary>
    /// Used to send command
    /// </summary>
    public class CommandAsyncEvent {
        private readonly BaseCommand _command;
        private readonly TaskCompletionSource<EslMessage> _source;

        public CommandAsyncEvent(BaseCommand command)
        {
            _command = command;
            _source = new TaskCompletionSource<EslMessage>();
        }

        /// <summary>
        /// The FreeSwitch command to  send
        /// </summary>
        public BaseCommand Command { get { return _command; } }

        /// <summary>
        /// The response
        /// </summary>
        public Task<EslMessage> Task { get { return _source.Task; } }

        public void Complete(EslMessage response) { _source.TrySetResult(response); }

    }
}