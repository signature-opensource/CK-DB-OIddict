using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;
using CK.DB.OpenIddictSql.Entities;
using FluentAssertions;
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
            var clientId = "ckdb-default-app";
            await ForceCreateApplicationAsync( clientId );
        }

        [Test]
        public async Task requesting_two_scopes_should_not_throwAsync()
        {
            var clientId = nameof( requesting_two_scopes_should_not_throwAsync );
            var applicationId = (await ForceCreateApplicationAsync( clientId )).ApplicationId;

            var authorizationManager =
            TestHelper.AutomaticServices.GetRequiredService<IOpenIddictAuthorizationManager>();

            var subject = "aymeric";
            var client = applicationId.ToString();
            var status = "valid";
            var type = "permanent";
            var scopes = new List<string> { "openid", "profile" }.ToImmutableArray();

            var authorizations = authorizationManager.FindAsync( subject, client, status, type, scopes ) as IAsyncEnumerable<Authorization>;
            await foreach( var authorization in authorizations! )
            {
                authorization.Scopes.Count.Should().Be( 2 );
            }
        }

        private async Task<Application> ForceCreateApplicationAsync( string clientId )
        {
            var applicationManager = TestHelper.AutomaticServices.GetRequiredService<IOpenIddictApplicationManager>();
            Debug.Assert( applicationManager != null, nameof( applicationManager ) + " != null" );

            var client = await applicationManager.FindByClientIdAsync( clientId );
            if( client != null )
            {
                await applicationManager.DeleteAsync( client );
            }

            var result = await applicationManager.CreateAsync
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

            var application = result as Application;
            return application;
        }
    }
}
