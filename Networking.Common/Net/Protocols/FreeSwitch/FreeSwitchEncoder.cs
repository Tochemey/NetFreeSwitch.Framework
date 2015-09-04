using System;
using System.Net.Sockets;
using System.Text;
using Networking.Common.Net.Channels;
using Networking.Common.Net.Protocols.FreeSwitch.Command;

namespace Networking.Common.Net.Protocols.FreeSwitch
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
            if (!(message is BaseCommand))
                throw new InvalidOperationException("This encoder only supports messages deriving from 'BaseCommand'");
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