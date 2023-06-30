using System.Collections.Generic;
using System.Collections.Immutable;
using System.Security.Claims;
using System.Threading.Tasks;
using CK.Auth;
using CK.DB.Actor;
using CK.DB.AspNet.OIddict.Identity;
using CK.SqlServer;
using OpenIddict.Abstractions;

namespace CK.DB.OIddict.DefaultServer.App
{
    public class WfaIdentityStrategy : IIdentityStrategy
    {
        private readonly IAuthenticationTypeSystem _authenticationTypeSystem;
        private readonly UserTable _userTable;

        public WfaIdentityStrategy
        (
            IAuthenticationTypeSystem authenticationTypeSystem,
            UserTable userTable
        )
        {
            _authenticationTypeSystem = authenticationTypeSystem;
            _userTable = userTable;
        }

        /// <inheritdoc />
        public async Task<AuthenticationInfo?> ValidateAuthAsync( ClaimsPrincipal? authResult )
        {
            var userName = authResult?.GetClaim( OpenIddictConstants.Claims.Name );
            var schemes = authResult?.GetClaim( Constants.Claims.Schemes );

            if( userName is null ) return default;

            int userId;

            using( var sqlCallContext = new SqlStandardCallContext() )
            {
                userId = await _userTable.FindByNameAsync( sqlCallContext, userName );
            }

            if( userId <= 0 )
                return default;

            var info = new Dictionary<string, string> { { Constants.Claims.Schemes, schemes! } };
            return new AuthenticationInfo( userName, userId, info );
        }

        /// <inheritdoc />
        public void SetUserClaims
        (
            ClaimsIdentity identity,
            AuthenticationInfo authenticationInfo,
            ImmutableArray<string> scopes
        )
        {
            var userName = authenticationInfo.UserName;
            var userId = authenticationInfo.UserId;
            var schemes = authenticationInfo.Info[Constants.Claims.Schemes];

            identity.SetClaim( OpenIddictConstants.Claims.Subject, userId )
                    .SetClaim( OpenIddictConstants.Claims.Email, userName )
                    .SetClaim( OpenIddictConstants.Claims.Username, userName )
                    .SetClaim( OpenIddictConstants.Claims.Name, userName );

            if( scopes.Contains( "authinfo" ) )
                identity.SetClaim( OpenIddictConstants.Claims.Subject, $"{userId}#{schemes}" );
        }
    }


    public static class Constants
    {
        public static class Claims
        {
            public const string Schemes = "schemes";
        }
    }
}
