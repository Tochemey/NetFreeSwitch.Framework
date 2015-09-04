﻿namespace Networking.Common.Net.Authentication.Messages {
    /// <summary>
    ///     Step one during the authentication steps.
    /// </summary>
    /// <remarks>
    ///     Should be sent to the server directly after a successful connect.
    /// </remarks>
    public interface IAuthenticationHandshake {
        /// <summary>
        ///     Name of the user that would like to authenticate
        /// </summary>
        string UserName { get; }
    }
}