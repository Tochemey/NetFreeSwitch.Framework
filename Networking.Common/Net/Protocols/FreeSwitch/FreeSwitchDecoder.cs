using System;
using System.Collections.Generic;
using System.Text;
using Networking.Common.Net.Channels;

namespace Networking.Common.Net.Protocols.FreeSwitch {
    /// <summary>
    ///     Decode incoming bytes into FreeSwitch Message
    /// </summary>
    public class FreeSwitchDecoder : IMessageDecoder {
        private const string MESSAGE_END_STRING = "\n\n";
        private const string EOL = "\r\n";
        private const char LINE_FEED_CHAR = '\n';

        private static byte[] _buffer;
        private static int _offsetInOurBuffer;
        private static int _bytesLeftInSocketBuffer;
        private readonly Encoding _encoding = Encoding.ASCII;
        private Action<object> _messageReceived;

        /// <summary>
        /// Reset decoder state so that we can decode a new message
        /// </summary>
        public void Clear() {
            _offsetInOurBuffer = 0;
            _bytesLeftInSocketBuffer = -1;
            _buffer = null;
        }

        /// <summary>
        ///     We've received bytes from the socket. Build a message out of them.
        /// </summary>
        /// <param name="buffer"></param>
        public void ProcessReadBytes(ISocketBuffer buffer) {
            // buffer offset received
            var offsetInSocketBuffer = buffer.Offset;

            // Number of Bytes received
             _bytesLeftInSocketBuffer = buffer.BytesTransferred;

            if (_buffer != null) {
                byte[] tmp = new byte[_buffer.Length];
                Buffer.BlockCopy(_buffer, 0, tmp, 0, _buffer.Length);
                _buffer = new byte[_buffer.Length + buffer.BytesTransferred];
                Buffer.BlockCopy(tmp, 0, _buffer, 0, tmp.Length);
                Buffer.BlockCopy(buffer.Buffer, 0, tmp, tmp.Length, buffer.BytesTransferred);
            }
            else {
                _buffer = new byte[buffer.BytesTransferred];
                // Move the byte read into our buffer
                Buffer.BlockCopy(buffer.Buffer, offsetInSocketBuffer, _buffer, _offsetInOurBuffer, buffer.BytesTransferred);
            }

            while (true) {
                if (_bytesLeftInSocketBuffer <= 0) break;

                _offsetInOurBuffer = 0;

                string textReceived = _encoding.GetString(_buffer);
                textReceived = textReceived.TrimStart(LINE_FEED_CHAR);
                if (!textReceived.Contains(MESSAGE_END_STRING)) {
                    // Read more bytes
                    _offsetInOurBuffer += textReceived.Length;
                    return;
                }

                // At this stage we have received at least the Headers.
                int contentLength = 0;
                string headerLine = textReceived.Substring(0, textReceived.IndexOf(MESSAGE_END_STRING, StringComparison.Ordinal));
                var headers = ReadHeaderLines(headerLine);
                if (headers != null
                    && headers.Count > 0)
                    if (headers.ContainsKey("Content-Length")) contentLength = int.Parse(headers["Content-Length"]);

                // Here let us check the validity of content length
                if (contentLength <= 0) {
                    // Message does not have content length only headers.
                    var decodedMessage = new EslDecodedMessage(headerLine, string.Empty, textReceived);
                    MessageReceived(decodedMessage);
                    Clear();

                    // Let us check whether there is another message in the buffer sent
                    textReceived = textReceived.Substring(textReceived.IndexOf(MESSAGE_END_STRING, StringComparison.Ordinal) + MESSAGE_END_STRING.Length);
                    if (!string.IsNullOrEmpty(textReceived)
                        && !textReceived.Equals(MESSAGE_END_STRING)
                        && !textReceived.Equals(EOL)) {
                        _buffer = _encoding.GetBytes(textReceived);
                    }
                }
                else {
                    // Message does have body as well as header.
                    string bodyLine = textReceived.Substring(textReceived.IndexOf(MESSAGE_END_STRING, StringComparison.Ordinal) + MESSAGE_END_STRING.Length);
                    if (string.IsNullOrEmpty(bodyLine)) {
                        // We need to read more bytes for the body
                        _offsetInOurBuffer += contentLength;
                        return;
                    }

                    // body has been read. However there are more to read to make it complete
                    if (bodyLine.Length < contentLength) {
                        // The body is not yet complete we need to read more
                        _offsetInOurBuffer += bodyLine.Length;
                        return;
                    }

                    // body has been fully read
                    if (contentLength == bodyLine.Length) {
                        MessageReceived(new EslDecodedMessage(headerLine, bodyLine, textReceived));
                        Clear();
                        return;
                    }

                    // There is another message in the buffer
                    if (bodyLine.Length > contentLength) {
                        int extraMessageLength = bodyLine.Length - contentLength;
                        string additionalMessage = bodyLine.Substring(extraMessageLength);
                        bodyLine = bodyLine.Substring(0, contentLength);
                        _buffer = _encoding.GetBytes(additionalMessage); // reset our buffer to the received bytes so that the additional message can be handled properly.
                        MessageReceived(new EslDecodedMessage(headerLine, bodyLine, textReceived));
                        Clear();
                        return;
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