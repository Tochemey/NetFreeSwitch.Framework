using System.Collections.Generic;

namespace Networking.Common.Net.Protocols.FreeSwitch.CallManager {
    /// <summary>
    /// Used to create originate application
    /// </summary>
    public abstract class BaseApplication : IEndPointAddress
    {
        /// <summary>
        /// Application name
        /// </summary>
        public abstract string ApplicationName { get; }

        /// <summary>
        /// Application arguments
        /// </summary>
        public abstract IList<string> Arguments { get; }

        public string ToDialString() { return Arguments != null ? string.Format("&{0}({1})", ApplicationName, string.Join(",", Arguments)) : string.Format("&{0}()", ApplicationName); }
    }
}