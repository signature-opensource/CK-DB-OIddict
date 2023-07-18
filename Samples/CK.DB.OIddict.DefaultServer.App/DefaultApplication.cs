using System;
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
        private readonly CommandAdapter<ICreateApplicationCommand, ISimpleCrisResult> _commandAdapter;

        public DefaultApplication
        (
            IOpenIddictApplicationManager applicationManager,
            UserTable userTable,
            IAuthenticationDatabaseService authenticationDatabaseService,
            CommandAdapter<ICreateApplicationCommand, ISimpleCrisResult> commandAdapter
        )
        {
            _applicationManager = applicationManager;
            _userTable = userTable;
            _authenticationDatabaseService = authenticationDatabaseService;
            _commandAdapter = commandAdapter;
        }

        public async Task EnsureAllDefaultAsync()
        {
            await EnsureDefaultUserAsync();
            await EnsureDefaultApplicationAsync();
        }

        private async Task EnsureDefaultApplicationAsync()
        {
            var app1Descriptor = new ApplicationDescriptorBuilder
                                 (
                                     "ckdb-default-app",
                                     "901564A5-E7FE-42CB-B10D-61EF6A8F3654"
                                 )
                                 .WithDisplayName( "CK-DB Default application" )
                                 .EnsureCodeDefaults()
                                 .AddRedirectUri( new Uri( "https://localhost:7273/callback/login/local" ) )
                                 .AddRedirectUri( new Uri( "https://oidcdebugger.com/debug" ) )
                                 .AddRedirectUri( new Uri( "https://localhost:5044/signin-oidc" ) )
                                 .AddPostLogoutRedirectUri( new Uri( "https://localhost:7273/callback/logout/local" ) )
                                 .AddScope( "authinfo" )
                                 .Build();
            var app2Descriptor = new ApplicationDescriptorBuilder
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
            var app3Descriptor = new ApplicationDescriptorBuilder
                                 (
                                     "app3",
                                     "901564A5-E7FE-42CB-B10D-61EF6A8F3654"
                                 )
                                 .WithDisplayName( "CK-DB Default application" )
                                 .EnsureCodeDefaults()
                                 .AddRedirectUri( new Uri( "https://localhost:7273/callback/login/local" ) )
                                 .AddRedirectUri( new Uri( "https://oidcdebugger.com/debug" ) )
                                 .AddRedirectUri( new Uri( "https://localhost:5044/signin-oidc" ) )
                                 .AddPostLogoutRedirectUri( new Uri( "https://localhost:7273/callback/logout/local" ) )
                                 .Build();

            var activityMonitor = new ActivityMonitor();

            await _commandAdapter.HandleAsync( activityMonitor, command => command.Descriptor = app1Descriptor );
            await _commandAdapter.HandleAsync( activityMonitor, command => command.Descriptor = app2Descriptor );
            await _commandAdapter.HandleAsync( activityMonitor, command => command.Descriptor = app3Descriptor );
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
