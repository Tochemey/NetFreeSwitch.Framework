﻿using System;
using System.Runtime.Serialization;

namespace Networking.Common.Net.Authentication.Messages {
    /// <summary>
    ///     Default implementation supporting both DataContract and the old .NET serializers.
    /// </summary>
    [DataContract, Serializable]
    public class AuthenticationHandshake : IAuthenticationHandshake {
        /// <summary>
        ///     Name of the user that would like to authenticate
        /// </summary>
        [DataMember(Order = 1)]
        public string UserName { get; set; }
    }
}