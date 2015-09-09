using System;
using System.Collections.Specialized;

namespace Networking.Common.Net.Protocols.FreeSwitch.Message {
    /// <summary>
    ///     FreeSwitch log/data wrapper
    /// </summary>
    public class LogData : EslMessage {
        /// <summary>
        ///     ApiResponse body
        /// </summary>
        public string Body { get { return Msg; } }

        public LogData(string message) : base(message) { }
        public LogData(NameValueCollection data, string msg) : base(data, msg) { }

        public EslLogLevels? Level
        {
            get
            {
                if (this["Log-Level"] == null)
                    return null;
                int num = int.Parse(this["Log-Level"]);
                if (num > (int) EslLogLevels.DEBUG)
                    return EslLogLevels.DEBUG;
                return (EslLogLevels) num;
            }
        }

        public string TextChannel { get { return this["Text-Channel"]; } }

        public string LogFile { get { return this["Log-File"]; } }

        public string LogFunc { get { return this["Log-Func"]; } }

        public string LogLine { get { return this["Log-Line"]; } }

        public string UserData { get { return this["User-Data"]; } }

        public string ContentLength { get { return this["Content-Length"]; } }

        public DateTime MsgTime { get { return DateTime.Parse(Body.Substring(0, Body.IndexOf("[", StringComparison.Ordinal)).Trim()); } }

        public string LogLineText { get { return Body.Substring(Body.IndexOf(LogFile + ":" + LogLine, StringComparison.Ordinal) + (LogFile + ":" + LogLine).Length); } }
    }
}