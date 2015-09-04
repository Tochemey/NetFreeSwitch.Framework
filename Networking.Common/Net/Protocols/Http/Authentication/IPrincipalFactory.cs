using System.Security.Principal;

namespace Networking.Common.Net.Protocols.Http.Authentication {
    /// <summary>
    ///     Used to create <see cref="IPrincipal" />
    /// </summary>
    public interface IPrincipalFactory {
        /// <summary>
        ///     Create a new prinicpal
        /// </summary>
        /// <param name="context">Context used to identify the user.</param>
        /// <returns>Principal to use</returns>
        IPrincipal Create(PrincipalFactoryContext context);
    }
}