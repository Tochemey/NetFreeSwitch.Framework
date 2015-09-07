using System.Threading;

namespace Networking.Common.Net.Protocols.FreeSwitch {
    /// <summary>
    ///     Holds information about FreeSwitch Events listener
    /// </summary>
    public class EventsListenInfo {
        /// <summary>
        ///     Current run state of the listening thread.
        /// </summary>
        public bool IsRunning = false;

        /// <summary>
        ///     The thread associated with listening.
        /// </summary>
        public Thread ListenThread;

        /// <summary>
        ///     This starts the listening thread
        /// </summary>
        /// <param name="ts">Starting function for new thread</param>
        public void Start(ThreadStart ts) {
            ListenThread = new Thread(ts);
            ListenThread.Start();
        }
    }
}