using System.Threading;

namespace Networking.Common.Net.Protocols.FreeSwitch {
    /// <summary>
    /// 
    /// </summary>
    public class SequenceGenerator {
        private static int _number;

        public static int Next() { return Interlocked.Increment(ref _number); }

    }
}