/*
Licensed to the Apache Software Foundation (ASF) under one
or more contributor license agreements.  See the NOTICE file
distributed with this work for additional information
regarding copyright ownership.  The ASF licenses this file
to you under the Apache License, Version 2.0 (the
"License"); you may not use this file except in compliance
with the License.  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing,
software distributed under the License is distributed on an
"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, either express or implied.  See the License for the
specific language governing permissions and limitations
under the License.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using NetFreeSwitch.Framework.Common;

namespace NetFreeSwitch.Framework.FreeSwitch {
    /// <summary>
    ///     EslDecodeMessage only holds the FreeSwitch message header lines and body lines.
    /// </summary>
    public class EslDecodedMessage {
        private readonly NameValueCollection _bodyLines;
        private readonly NameValueCollection _headers;

        public EslDecodedMessage(string headers, string bodyLines, string originalMessage) {
            OriginalMessage = originalMessage;
            HeaderText = headers;
            BodyText = bodyLines;
            _bodyLines = ParseBody(bodyLines);
            _headers = ParseHeader(headers);
        }

        public NameValueCollection Headers { get { return _headers; } }
        public NameValueCollection BodyLines { get { return _bodyLines; } }
        public string HeaderText { get; private set; }
        public string BodyText { get; private set; }
        public string OriginalMessage { get; private set; }

        private NameValueCollection ParseHeader(string headerLines) {
            var items = new Dictionary<string, string>();
            var parser = new EslStringReader(headerLines);
            string name = parser.Read(':', true);
            while (!parser.EOF
                   && name != string.Empty) {
                string value = parser.ReadLine(true);
                if (!items.ContainsKey(name.Trim())) {
                    try { items.Add(name.Trim(), Uri.UnescapeDataString(value.Trim())); }
                    catch (UriFormatException) {
                        // add the value unformatted
                        items.Add(name.Trim(), value.Trim());
                    }
                }

                name = parser.Read(':', true);
            }

            return items.ToNameValueCollection();
        }

        private NameValueCollection ParseBody(string bodyLines) {
            var items = new Dictionary<string, string>();
            var parser = new EslStringReader(bodyLines);
            string name = parser.Read(':', true);
            while (!parser.EOF
                   && !string.IsNullOrEmpty(name)) {
                string value = parser.ReadLine(true);
                if (!items.ContainsKey(name.Trim())) {
                    try { items.Add(name.Trim(), Uri.UnescapeDataString(value.Trim())); }
                    catch (UriFormatException) {
                        // add the value unformatted
                        items.Add(name.Trim(), value.Trim());
                    }
                }
                name = parser.Read(':', true);
            }

            var extraContents = new List<string>();
            while (!parser.EOF) {
                string line = parser.ReadLine(true);
                if (!string.IsNullOrEmpty(line)) extraContents.Add(line.Trim());
            }

            if (extraContents.Count <= 0) return items.ToNameValueCollection();
            if (!items.ContainsKey("__CONTENT__")) items.Add("__CONTENT__", String.Join("|", extraContents));
            return items.ToNameValueCollection();
        }
    }
}
