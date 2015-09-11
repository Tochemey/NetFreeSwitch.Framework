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
using System.Net.Sockets;
using System.Text;
using NetFreeSwitch.Framework.Common;
using NetFreeSwitch.Framework.FreeSwitch.Commands;
using NetFreeSwitch.Framework.Net;
using NetFreeSwitch.Framework.Net.Channels;

namespace NetFreeSwitch.Framework.FreeSwitch
{
    public class FreeSwitchEncoder : IMessageEncoder {
        private const string MESSAGE_END_STRING = "\n\n";
        private BaseCommand _message;
        private readonly byte[] _buffer = new byte[65535];
        private int _bytesToSend;
        private int _offset;


        /// <summary>
        ///     Are about to send a new message
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <remarks>
        ///     Can be used to prepare the next message. for instance serialize it etc.
        /// </remarks>
        /// <exception cref="NotSupportedException">Message is of a type that the encoder cannot handle.</exception>
        public void Prepare(object message) {
            if (!(message is BaseCommand)) {
               // throw new InvalidOperationException("This encoder only supports messages deriving from 'BaseCommand'");
                return;
            }
            _message = (BaseCommand) message;

            string command = _message.ToString();
            if (string.IsNullOrEmpty(command)) throw new InvalidOperationException("The encoder cannot encode the message. Nothing to encode.");
            if (!command.Trim().EndsWith(MESSAGE_END_STRING)) command += MESSAGE_END_STRING;

            var len = Encoding.UTF8.GetByteCount(command);
            BitConverter2.GetBytes(len, _buffer, 0);
            Encoding.UTF8.GetBytes(command, 0, command.Length, _buffer, 0);
            _bytesToSend = len;
            _offset = 0;
        }

        /// <summary>
        ///     Buffer structure used for socket send operations.
        /// </summary>
        /// <param name="buffer">
        ///     Do note that there are not buffer attached to the structure, you have to assign one yourself using
        ///     <see cref="ISocketBuffer.SetBuffer(int,int)" />. This choice was made
        ///     to prevent unnecessary copy operations.
        /// </param>
        /// <remarks>
        ///     The <c>buffer</c> variable is typically a wrapper around <see cref="SocketAsyncEventArgs" />, but may be something
        ///     else if required.
        /// </remarks>
        public void Send(ISocketBuffer buffer) { buffer.SetBuffer(_buffer, _offset, _bytesToSend); }

        /// <summary>
        ///     The previous <see cref="IMessageEncoder.Send" /> has just completed.
        /// </summary>
        /// <param name="bytesTransferred"></param>
        /// <remarks><c>true</c> if the message have been sent successfully; otherwise <c>false</c>.</remarks>
        public bool OnSendCompleted(int bytesTransferred) {
            _offset += bytesTransferred;
            _bytesToSend -= bytesTransferred;
            return _bytesToSend <= 0;
        }

        /// <summary>
        ///     Remove everything used for the last message
        /// </summary>
        public void Clear() {
            _bytesToSend = 0;
            _offset = 0;
        }
    }
}
