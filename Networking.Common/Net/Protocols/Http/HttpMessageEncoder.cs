﻿using System;
using System.IO;
using Networking.Common.Net.Channels;

namespace Networking.Common.Net.Protocols.Http {
    /// <summary>
    ///     Used to encode request/response into a byte stream.
    /// </summary>
    public class HttpMessageEncoder : IMessageEncoder {
        private readonly byte[] _buffer = new byte[65535];
        private readonly object _resetLock = new object();
        private readonly MemoryStream _stream;
        private readonly StreamWriter _writer;
        private int _bytesToSend;
        private bool _isHeaderSent;
        private HttpMessage _message;
        private int _offset;
        private int _totalAmountToSend;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpMessageEncoder" /> class.
        /// </summary>
        public HttpMessageEncoder() {
            _stream = new MemoryStream(_buffer);
            _stream.SetLength(0);
            _writer = new StreamWriter(_stream);
        }

        /// <summary>
        ///     Are about to send a new message
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <remarks>
        ///     Can be used to prepare the next message. for instance serialize it etc.
        /// </remarks>
        /// <exception cref="NotSupportedException">Message is of a type that the encoder cannot handle.</exception>
        public void Prepare(object message) {
            if (!(message is HttpMessage))
                throw new InvalidOperationException("This encoder only supports messages deriving from 'HttpMessage'");

            _message = (HttpMessage) message;
            if (_message.Body == null
                || _message.Body.Length == 0)
                _message.Headers["Content-Length"] = "0";
            else if (_message.ContentLength == 0) {
                _message.ContentLength = (int) _message.Body.Length;
                if (_message.Body.Position == _message.Body.Length)
                    _message.Body.Position = 0;
            }
        }

        /// <summary>
        ///     Buffer structure used for socket send operations.
        /// </summary>
        /// <param name="buffer">
        ///     Do note that there are not buffer attached to the structure, you have to assign one yourself using
        ///     <see cref="ISocketBuffer.SetBuffer(int,int)" />. This choice was made
        ///     to prevent unnecessary copy operations.
        /// </param>
        public void Send(ISocketBuffer buffer) {
            // last send operation did not send all bytes enqueued in the buffer
            // so let's just continue until doing next message
            if (_bytesToSend > 0) {
                buffer.SetBuffer(_buffer, _offset, _bytesToSend);
                return;
            }

            // continuing with the message body
            if (_isHeaderSent) {
                var bytes = Math.Min(_totalAmountToSend, _buffer.Length);
                _message.Body.Read(_buffer, 0, bytes);
                _bytesToSend = bytes;

                buffer.SetBuffer(_buffer, 0, bytes);
                return;
            }

            _writer.WriteLine(_message.StatusLine);
            foreach (var header in _message.Headers) _writer.Write("{0}: {1}\r\n", header.Key, header.Value);
            _writer.Write("\r\n");
            _writer.Flush();
            _isHeaderSent = true;
            buffer.UserToken = _message;

            if (_message.Body == null
                || _message.ContentLength == 0) {
                _bytesToSend = (int) _stream.Length;
                _totalAmountToSend = _bytesToSend;
                buffer.SetBuffer(_buffer, 0, (int) _stream.Length);
                return;
            }

            var bytesLeft = _buffer.Length - _stream.Length;
            var bytesToSend = Math.Min(_message.ContentLength, (int) bytesLeft);
            var offset = (int) _stream.Position;
            _message.Body.Read(_buffer, offset, bytesToSend);
            _bytesToSend = (int) _stream.Length + bytesToSend;
            _totalAmountToSend = (int) _stream.Length + _message.ContentLength;
            buffer.SetBuffer(_buffer, 0, _bytesToSend);
        }

        /// <summary>
        ///     The previous <see cref="IMessageEncoder.Send" /> has just completed.
        /// </summary>
        /// <param name="bytesTransferred"></param>
        /// <remarks><c>true</c> if the message have been sent successfully; otherwise <c>false</c>.</remarks>
        public bool OnSendCompleted(int bytesTransferred) {
            _totalAmountToSend -= bytesTransferred;
            _bytesToSend -= bytesTransferred;
            _offset += bytesTransferred;

            if (_bytesToSend <= 0)
                _offset = 0;

            if (_totalAmountToSend == 0) Clear();

            return _totalAmountToSend <= 0;
        }

        /// <summary>
        ///     Remove everything used for the last message
        /// </summary>
        public void Clear() {
            if (_message != null) {
                lock (_resetLock) {
                    if (_message != null
                        && _message.Body != null)
                        _message.Body.Dispose();

                    _message = null;
                }
            }

            _bytesToSend = 0;
            _isHeaderSent = false;
            _stream.SetLength(0);
        }
    }
}