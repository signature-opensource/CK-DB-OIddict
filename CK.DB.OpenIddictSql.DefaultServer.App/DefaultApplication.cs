using System;
using System.Linq;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.Actor;
using CK.DB.Auth;
using CK.SqlServer;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants.ConsentTypes;
using static OpenIddict.Abstractions.OpenIddictConstants.Permissions;
using static OpenIddict.Abstractions.OpenIddictConstants.Requirements;

namespace CK.DB.OpenIddictSql.DefaultServer.App
{
    public class DefaultApplication : IAutoService
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly UserTable _userTable;
        private readonly IAuthenticationDatabaseService _authenticationDatabaseService;

        public DefaultApplication
        (
            IOpenIddictApplicationManager applicationManager,
            UserTable userTable,
            IAuthenticationDatabaseService authenticationDatabaseService
        )
        {
            _applicationManager = applicationManager;
            _userTable = userTable;
            _authenticationDatabaseService = authenticationDatabaseService;
        }

        public async Task EnsureAllDefaultAsync()
        {
            await EnsureDefaultUserAsync();
            await EnsureDefaultApplicationAsync();
        }

        private async Task EnsureDefaultApplicationAsync()
        {
            var clientId = "ckdb-default-app";
            var client = await _applicationManager.FindByClientIdAsync( clientId );
            if( client != null )
            {
                return;
            }

            await _applicationManager.CreateAsync
            (
                new OpenIddictApplicationDescriptor
                {
                    ClientId = clientId,
                    ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",
                    ConsentType = Explicit,
                    DisplayName = "CK-DB Default application",
                    RedirectUris =
                    {
                        new Uri( "https://localhost:7273/callback/login/local" ),
                        new Uri( "https://oidcdebugger.com/debug" ),
                        new Uri( "https://localhost:5044/signin-oidc" ),
                    },
                    PostLogoutRedirectUris =
                    {
                        new Uri( "https://localhost:7273/callback/logout/local" ),
                    },
                    Permissions =
                    {
                        Endpoints.Authorization,
                        Endpoints.Logout,
                        Endpoints.Token,
                        GrantTypes.AuthorizationCode,
                        ResponseTypes.Code,
                        Scopes.Email,
                        Scopes.Profile,
                        Scopes.Roles,
                    },
                    Requirements =
                    {
                        Features.ProofKeyForCodeExchange,
                    },
                }
            );
        }

        public async Task<string> GetDefaultApplicationInfoAsync()
        {
            var clientId = "ckdb-default-app";
            var client = (await _applicationManager.FindByClientIdAsync( clientId )) as OpenIddictApplicationDescriptor;

            var result = $@"
ClientId : {client?.ClientId}
ClientSecret : {client?.ClientSecret}
Permissions : {client?.Permissions.Select( c => c ).Concatenate( " " )}
Requirements : {client?.Requirements.Select( c => c ).Concatenate( " " )}
ConsentType : {client?.ConsentType}
DisplayName : {client?.DisplayName}
Properties : {client?.Properties.Select( c => $"{c.Key}{c.Value.ToString()}" ).Concatenate( " " )}
RedirectUris : {client?.RedirectUris.Select( c => c.ToString() ).Concatenate( " " )}
PostLogoutRedirectUris : {client?.PostLogoutRedirectUris.Select( c => c.ToString() ).Concatenate( " " )}
";
            return result;
        }

        private async Task EnsureDefaultUserAsync()
        {
            using var context = new SqlStandardCallContext();

            var userId = await _userTable.FindByNameAsync( context, "Aymeric" );

            if( userId > 0 ) return;
            userId = await _userTable.CreateUserAsync( context, 1, "Aymeric" );
            var uclResult = await _authenticationDatabaseService
                                  .BasicProvider
                                  .CreateOrUpdatePasswordUserAsync( context, 1, userId, "passwd" );
        }
    }
}
