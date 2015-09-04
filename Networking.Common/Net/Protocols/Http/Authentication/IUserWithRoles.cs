using System.Security.Principal;

namespace Networking.Common.Net.Protocols.Http.Authentication {
    /// <summary>
    ///     Used to be able to generate <see cref="IPrincipal" /> directly.
    /// </summary>
    public interface IUserWithRoles : IAuthenticationUser {
        /// <summary>
        ///     Get a list of all roles
        /// </summary>
        string[] RoleNames { get; }
    }
}