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
using System.Text;
using NetFreeSwitch.Framework.Net;
using NetFreeSwitch.Framework.Net.Channels;

namespace NetFreeSwitch.Framework.FreeSwitch {
    /// <summary>
    ///     Decode incoming bytes into FreeSwitch Message
    /// </summary>
    public class FreeSwitchDecoder : IMessageDecoder {
        private const string MESSAGE_END_STRING = "\n\n";
        private const string EOL = "\r\n";
        private const char LINE_FEED_CHAR = '\n';

        private static int _bytesReceived;
        private readonly Encoding _encoding = Encoding.ASCII;
        private Action<object> _messageReceived;
        private static readonly byte[] _buffer = new byte[65535];
        private static int _offsetInOurBuffer;

        public FreeSwitchDecoder() {
            _messageReceived = o => { };
            _offsetInOurBuffer = 0;
        }

        /// <summary>
        ///     Reset decoder state so that we can decode a new message
        /// </summary>
        public void Clear() {
            _bytesReceived = -1;
            Array.Clear(_buffer, 0, _buffer.Length);
        }

        /// <summary>
        ///     We've received bytes from the socket. Build a message out of them.
        /// </summary>
        /// <param name="buffer"></param>
        public void ProcessReadBytes(ISocketBuffer buffer) {
            // buffer offset received
            var offsetInSocketBuffer = buffer.Offset;

            // Number of Bytes received
            _bytesReceived = buffer.BytesTransferred;
            while (true) {
                if (_bytesReceived <= 0) break;
                Buffer.BlockCopy(buffer.Buffer, offsetInSocketBuffer, _buffer, _offsetInOurBuffer, buffer.BytesTransferred);

                string _textReceived = _encoding.GetString(_buffer);
                _textReceived = _textReceived.TrimStart(LINE_FEED_CHAR).Replace("\0", string.Empty);
                if (!_textReceived.Contains(MESSAGE_END_STRING)) {
                    _offsetInOurBuffer += _bytesReceived;
                    return;
                }

                int contentLength = 0;
                string headerLine = _textReceived.Substring(0, _textReceived.IndexOf(MESSAGE_END_STRING, StringComparison.Ordinal));
                var headers = ReadHeaderLines(headerLine);
                if (headers != null
                    && headers.Count > 0)
                    if (headers.ContainsKey("Content-Length")) contentLength = int.Parse(headers["Content-Length"]);

                int len;
                if (contentLength <= 0) {
                    // Message does not have content length only headers.
                    var decodedMessage = new EslDecodedMessage(headerLine, string.Empty, _textReceived);
                    MessageReceived(decodedMessage);
                    Clear();

                    // Let us check whether there is another message in the buffer sent
                    _textReceived = _textReceived.Substring(_textReceived.IndexOf(MESSAGE_END_STRING, StringComparison.Ordinal) + MESSAGE_END_STRING.Length);
                    if (string.IsNullOrEmpty(_textReceived)
                        || _textReceived.Equals(MESSAGE_END_STRING)
                        || _textReceived.Equals(EOL)) continue;
                    // Length of extra bytes
                    len = _encoding.GetByteCount(_textReceived);
                    _encoding.GetBytes(_textReceived, 0, _textReceived.Length, _buffer, 0);
                    _offsetInOurBuffer += len;
                }
                else {
                    // Message does have body as well as header.
                    string bodyLine = _textReceived.Substring(_textReceived.IndexOf(MESSAGE_END_STRING, StringComparison.Ordinal) + MESSAGE_END_STRING.Length);
                    if (string.IsNullOrEmpty(bodyLine)) {
                        len = _encoding.GetByteCount(_textReceived);
                        // We need to read more bytes for the body    
                        _offsetInOurBuffer += len;
                        return;
                    }

                    // body has been read. However there are more to read to make it complete
                    if (bodyLine.Length < contentLength) {
                        // get the count of the received bytes
                        len = _encoding.GetByteCount(_textReceived);
                        // The body is not yet complete we need to read more  
                        _offsetInOurBuffer += len;
                        return;
                    }

                    // body has been fully read
                    if (contentLength == bodyLine.Length) {
                        MessageReceived(new EslDecodedMessage(headerLine, bodyLine, _textReceived));
                        Clear();
                        return;
                    }

                    // There is another message in the buffer
                    if (bodyLine.Length > contentLength) {
                        string bodyLine2 = bodyLine.Substring(0, contentLength);
                        MessageReceived(new EslDecodedMessage(headerLine, bodyLine2, headerLine+ bodyLine2));
                        Clear();

                        _textReceived = bodyLine.Remove(bodyLine.IndexOf(bodyLine2, StringComparison.Ordinal), bodyLine2.Length);
                        if (string.IsNullOrEmpty(_textReceived)
                            || _textReceived.Equals(MESSAGE_END_STRING)
                            || _textReceived.Equals(EOL)) return;
                        // Length of extra bytes
                        len = _encoding.GetByteCount(_textReceived);
                        _encoding.GetBytes(_textReceived, 0, _textReceived.Length, _buffer, 0);
                        _offsetInOurBuffer += len;
                    }
                }
            }
        }

        /// <summary>
        ///     A message has been received.
        /// </summary>
        /// <remarks>
        ///     Do note that streams are being reused by the decoder, so don't try to close it.
        /// </remarks>
        public Action<object> MessageReceived
        {
            get { return _messageReceived; }
            set
            {
                if (value == null)
                    value = o => { };

                _messageReceived = value;
            }
        }

        private Dictionary<string, string> ReadHeaderLines(string headerLine) {
            if (string.IsNullOrEmpty(headerLine))
                return new Dictionary<string, string>();
            var ret = new Dictionary<string, string>();
            foreach (string str in headerLine.Split('\n')) {
                if ((str.Length <= 0)
                    || str.StartsWith("#")
                    || !str.Contains(":"))
                    continue;
                string name = str.Substring(0, str.IndexOf(":", StringComparison.Ordinal));
                string value = str.Substring(str.IndexOf(":", StringComparison.Ordinal) + 1);
                if (value.Contains("#")) {
                    for (int x = 0; x < value.Length; x++) {
                        if (value[x] != '#') continue;
                        if ((x > 0)
                            && (value[x - 1] != '\\')) {
                            value = value.Substring(0, x);
                            break;
                        }
                        if (x == 0)
                            value = "";
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
