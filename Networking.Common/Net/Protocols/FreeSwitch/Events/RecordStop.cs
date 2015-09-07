using System;
using System.Collections.Generic;

namespace Networking.Common.Net.Protocols.FreeSwitch.Events {
    public class RecordStop : EslEvent {
        public RecordStop(string message) : base(message) { }
        public RecordStop(string message, string eventMessage) : base(message, eventMessage) { }

        public RecordStop(Dictionary<string, string> parameters) { SetParameters(parameters); }

        public RecordStop() { }

        public string RecordFilePath { get { return this["Record-File-Path"]; } }

        public int RecordSeconds { get { return string.IsNullOrEmpty(this["record_seconds"]) ? 0 : this["record_seconds"].IsNumeric() ? Convert.ToInt32(this["record_seconds"]) : 0; } }

        public int RecordMilliSeconds { get { return string.IsNullOrEmpty(this["record_ms"]) ? 0 : this["record_ms"].IsNumeric() ? Convert.ToInt32(this["record_ms"]) : 0; } }
    }
}