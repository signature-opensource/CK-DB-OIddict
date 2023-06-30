using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CK.DB.OIddict.Entities;
using CK.Testing;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;

namespace CK.DB.OIddict.Tests
{
    public class ApplicationTestHelper
    {
        internal async Task<Application> ForceCreateApplicationAsync( string clientId )
        {
            var applicationManager = DBSetupTestHelper.TestHelper.AutomaticServices.GetRequiredService<IOpenIddictApplicationManager>();
            var authorizationManager =
            DBSetupTestHelper.TestHelper.AutomaticServices.GetRequiredService<IOpenIddictAuthorizationManager>();
            Debug.Assert( applicationManager != null, nameof( applicationManager ) + " != null" );

            var client = await applicationManager.FindByClientIdAsync( clientId );
            if( client != null )
            {
                var authorizations = authorizationManager.FindByApplicationIdAsync
                (
                    (client as Application).ApplicationId.ToString()
                );
                await foreach( var authorization in authorizations )
                {
                    await authorizationManager.DeleteAsync( authorization );
                }

                await applicationManager.DeleteAsync( client );
            }

            var result = await applicationManager.CreateAsync
            (
                new OpenIddictApplicationDescriptor
                {
                    ClientId = clientId,
                    ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",
                    ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                    DisplayName = "CK-DB Default application",
                    RedirectUris =
                    {
                        new Uri( "https://localhost:7273/callback/login/local" ),
                    },
                    PostLogoutRedirectUris =
                    {
                        new Uri( "https://localhost:7273/callback/logout/local" ),
                    },
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Logout,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.ResponseTypes.Code,
                        OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Roles,
                    },
                    Requirements =
                    {
                        OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange,
                    },
                }
            );

            var application = result as Application;
            return application;
        }
    }
}
