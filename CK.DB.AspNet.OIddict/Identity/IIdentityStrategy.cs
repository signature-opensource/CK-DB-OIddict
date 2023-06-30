using System.Collections.Immutable;
using System.Security.Claims;
using System.Threading.Tasks;
using CK.Core;

namespace CK.DB.AspNet.OIddict.Identity
{
    public interface IIdentityStrategy : IAutoService
    {
        /// <summary>
        /// Validate user against a storage like a database. Populate AuthenticationInfo with information that may be
        /// used in <see cref="SetUserClaims"/>.
        /// </summary>
        /// <param name="authResult">Return null if user is invalid.</param>
        /// <returns></returns>
        Task<AuthenticationInfo?> ValidateAuthAsync( ClaimsPrincipal? authResult );

        /// <summary>
        /// Add claims based on authInfo, eventually based on scopes.
        /// </summary>
        /// <param name="identity">The current user.</param>
        /// <param name="authInfo">AuthenticationInfo created from <see cref="ValidateAuthAsync"/>.</param>
        /// <param name="scopes">OIDC scopes for the current request.</param>
        void SetUserClaims( ClaimsIdentity identity, AuthenticationInfo authInfo, ImmutableArray<string> scopes );
    }
}
