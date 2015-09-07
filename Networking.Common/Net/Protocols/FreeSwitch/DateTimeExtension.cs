using System;

namespace Networking.Common.Net.Protocols.FreeSwitch {
    /// <summary>
    /// Extension methods for DateTime
    /// </summary>
    public static class DateTimeExtension {
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

        public static DateTime FromUnixTime(this string value)
        {
            long time;
            return long.TryParse(value, out time) ? UnixEpoch.AddMilliseconds(Convert.ToDouble(time) / 1000) : DateTime.MinValue;
        }

    }
}