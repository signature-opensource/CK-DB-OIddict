using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.Actor;
using CK.DB.Auth;
using CK.DB.OIddict.Commands;
using CK.SqlServer;
using OpenIddict.Abstractions;

namespace CK.DB.OIddict.DefaultServer.App
{
    public class DefaultApplication : IAutoService
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly UserTable _userTable;
        private readonly IAuthenticationDatabaseService _authenticationDatabaseService;
        private readonly CommandAdapter<ICreateApplicationCommand, ISimpleCrisResult> _createAppCommandAdapter;
        private readonly CommandAdapter<ICreateCodeFlowApplicationCommand, ISimpleCrisResult> _createCodeCommandAdapter;

        private readonly PocoDirectory _pocoDirectory;

        public DefaultApplication
        (
            IOpenIddictApplicationManager applicationManager,
            UserTable userTable,
            IAuthenticationDatabaseService authenticationDatabaseService,
            CommandAdapter<ICreateApplicationCommand, ISimpleCrisResult> createAppCommandAdapter,
            CommandAdapter<ICreateCodeFlowApplicationCommand, ISimpleCrisResult> createCodeCommandAdapter,
            PocoDirectory pocoDirectory
        )
        {
            _applicationManager = applicationManager;
            _userTable = userTable;
            _authenticationDatabaseService = authenticationDatabaseService;
            _createAppCommandAdapter = createAppCommandAdapter;
            _createCodeCommandAdapter = createCodeCommandAdapter;
            _pocoDirectory = pocoDirectory;
        }

        public async Task EnsureAllDefaultAsync()
        {
            await EnsureDefaultUserAsync();
            await EnsureDefaultApplicationAsync();
        }

        private async Task EnsureDefaultApplicationAsync()
        {
            var activityMonitor = new ActivityMonitor();

            var appPoco = _pocoDirectory.Create<IApplicationPoco>
            (
                ap =>
                {
                    ap.ClientId = "ckdb-default-app";
                    ap.ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654";
                    ap.ConsentType = OpenIddictConstants.ConsentTypes.Explicit;
                    ap.DisplayName = "CK-DB Default application";
                    ap.RedirectUris = new HashSet<IUriPoco>
                    {
                        _pocoDirectory.Create<IUriPoco>( up => up.Uri = "https://localhost:7273/callback/login/local" ),
                        _pocoDirectory.Create<IUriPoco>( up => up.Uri = "https://localhost:5044/signin-oidc" ),
                    };
                    ap.PostLogoutRedirectUris = new HashSet<IUriPoco>
                    {
                        _pocoDirectory.Create<IUriPoco>
                        (
                            up => up.Uri = "https://localhost:7273/callback/logout/local"
                        ),
                    };
                    ap.Permissions = new HashSet<string>
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Logout,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.ResponseTypes.Code,
                        OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Roles,
                        $"{OpenIddictConstants.Permissions.Prefixes.Scope}authinfo",
                    };
                    ap.Requirements = new HashSet<string>
                    {
                        OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange,
                    };
                }
            );

            await _createAppCommandAdapter.HandleAsync
            (
                activityMonitor,
                command => command.ApplicationPoco = appPoco
            );

            await _createCodeCommandAdapter.HandleAsync
            (
                activityMonitor,
                command =>
                {
                    command.ClientId = "ckdb-code-flow-easy";
                    command.ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654";
                    command.DisplayName = "My code flow app";
                    command.RedirectUri = _pocoDirectory
                    .Create<IUriPoco>( up => up.Uri = "https://localhost:5044/signin-oidc" );
                }
            );

            var appDescriptor = new ApplicationDescriptorBuilder
                                 (
                                     "anOtherApp",
                                     "901564A5-E7FE-42CB-B10D-61EF6A8F3654"
                                 )
                                 .WithDisplayName( "CK-DB Default application" )
                                 .EnsureCodeDefaults()
                                 .AddRedirectUri( new Uri( "https://localhost:7273/callback/login/local" ) )
                                 .AddRedirectUri( new Uri( "https://oidcdebugger.com/debug" ) )
                                 .AddRedirectUri( new Uri( "https://localhost:5044/signin-oidc" ) )
                                 .AddPostLogoutRedirectUri( new Uri( "https://localhost:7273/callback/logout/local" ) )
                                 .Build();

            if( await _applicationManager.FindByClientIdAsync( appDescriptor.ClientId! ) == null )
                await _applicationManager.CreateAsync( appDescriptor );
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
