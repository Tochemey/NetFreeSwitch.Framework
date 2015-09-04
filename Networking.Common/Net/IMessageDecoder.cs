using System;
using Networking.Common.Net.Channels;

namespace Networking.Common.Net {
    /// <summary>
    ///     Decodes incoming bytes into something more useful
    /// </summary>
    public interface IMessageDecoder {
        /// <summary>
        ///     A message has been received.
        /// </summary>
        /// <remarks>
        ///     Do note that streams are being reused by the decoder, so don't try to close it.
        /// </remarks>
        Action<object> MessageReceived { get; set; }

        /// <summary>
        ///     Clear state to allow this decoder to be reused.
        /// </summary>
        void Clear();

        /// <summary>
        ///     We've received bytes from the socket. Build a message out of them.
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <remarks></remarks>
        void ProcessReadBytes(ISocketBuffer buffer);
    }
}