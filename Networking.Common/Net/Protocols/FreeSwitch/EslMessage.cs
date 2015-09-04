using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Networking.Common.Net.Protocols.FreeSwitch {
    /// <summary>
    ///     FreeSwitch Message wrapper
    /// </summary>
    public class EslMessage {
        private readonly string _originalMessage;

        public EslMessage(string origMessage) {
            Items = new Dictionary<string, string>();
            _originalMessage = origMessage;
        }

        public EslMessage(NameValueCollection data, string origMessage) {
            _originalMessage = origMessage;
            Items = new Dictionary<string, string>();
            foreach (var k in data.AllKeys) {
                if(!Items.ContainsKey(k))
                Items.Add(k, data[k]);
            }
        }

        public Dictionary<string, string>.KeyCollection Keys { get { return Items.Keys; } }

        public Dictionary<string, string> Items { get; private set; }

        public string OriginalMessage { get { return _originalMessage; } }

        public string this[string name]
        {
            get
            {
                if (Items.ContainsKey(name))
                    return Uri.UnescapeDataString(Items[name]);
                if (Items.ContainsKey("variable_" + name))
                    return Uri.UnescapeDataString(Items["variable_" + name]);
                return null;
            }
        }

        protected void CopyParameters(ref Dictionary<string, string> parameters) {
            foreach (string str in Items.Keys) {
                if (parameters.ContainsKey(str))
                    parameters.Remove(str);
                parameters.Add(str, Uri.UnescapeDataString(Items[str]));
            }
        }

        protected void SetParameters(Dictionary<string, string> parameters) {
            foreach (string key in parameters.Keys) {
                if (!Items.ContainsKey(key.Trim()))
                    Items.Add(key.Trim(), parameters[key.Trim()].Trim());
                else Items.Remove(key.Trim());
            }
        }

        protected Dictionary<string, string> Read(string rawMessage) {
            if (rawMessage == null)
                return new Dictionary<string, string>();
            var ret = new Dictionary<string, string>();
            foreach (string str in rawMessage.Split('\n')) {
                if ((str.Length <= 0)
                    || str.StartsWith("#")
                    || !str.Contains(":")) continue;
                string name = str.Substring(0, str.IndexOf(":", StringComparison.Ordinal));
                string value = str.Substring(str.IndexOf(":", StringComparison.Ordinal) + 1);
                if (value.Contains("#")) {
                    for (int x = 0; x < value.Length; x++) {
                        if (value[x] == '#') {
                            if ((x > 0)
                                && (value[x - 1] != '\\')) {
                                value = value.Substring(0, x);
                                break;
                            }
                            if (x == 0)
                                value = "";
                        }
                    }
                }
                if (value.Length <= 0) continue;
                if (ret.ContainsKey(name.Trim())) ret.Remove(name.Trim());
                ret.Add(name.Trim(), value.Trim());
            }
            return ret;
        }
    }
}