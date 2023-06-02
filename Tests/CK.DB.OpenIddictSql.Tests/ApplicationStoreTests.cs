using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenIddict.Abstractions;
using static CK.Testing.DBSetupTestHelper;
using static OpenIddict.Abstractions.OpenIddictConstants.ConsentTypes;
using static OpenIddict.Abstractions.OpenIddictConstants.Permissions;
using static OpenIddict.Abstractions.OpenIddictConstants.Requirements;

namespace CK.DB.OpenIddictSql.Tests
{
    public class ApplicationStoreTests
    {
        [Test]
        public async Task should_create_new_application_Async()
        {
            var applicationManager = TestHelper.AutomaticServices.GetRequiredService<IOpenIddictApplicationManager>();

            Debug.Assert( applicationManager != null, nameof( applicationManager ) + " != null" );

            var clientId = "ckdb-default-app";
            var client = await applicationManager.FindByClientIdAsync( clientId );
            if( client != null )
            {
                await applicationManager.DeleteAsync( client );
            }

            await applicationManager.CreateAsync
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
                        OpenIddictConstants.GrantTypes.AuthorizationCode,
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
    }
}
