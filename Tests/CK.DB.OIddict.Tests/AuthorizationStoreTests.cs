using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CK.DB.OIddict.Entities;
using CK.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenIddict.Abstractions;

namespace CK.DB.OIddict.Tests
{
    public class AuthorizationStoreTests
    {
        private readonly ApplicationTestHelper _applicationTestHelper = new ApplicationTestHelper();

        [Test]
        public async Task requesting_two_scopes_exact_match_in_any_order_should_not_throw_Async()
        {
            var authorizationManager =
            DBSetupTestHelper.TestHelper.AutomaticServices.GetRequiredService<IOpenIddictAuthorizationManager>();

            var clientId = nameof( requesting_two_scopes_exact_match_in_any_order_should_not_throw_Async );
            var applicationId = (await _applicationTestHelper.ForceCreateApplicationAsync( clientId )).ApplicationId;
            var subject = "aymeric";
            var client = applicationId.ToString();
            var status = "valid";
            var type = "permanent";
            var scopes = new List<string> { "openid", "profile" }.ToImmutableArray();

            await authorizationManager.CreateAsync
            (
                identity: new ClaimsIdentity(),
                subject: subject,
                client: applicationId.ToString(),
                type: OpenIddictConstants.AuthorizationTypes.Permanent,
                scopes: scopes
            );

            var authorizations = authorizationManager.FindAsync( subject, client, status, type, scopes );
            var count = 0;
            await foreach( var authorization in (authorizations as IAsyncEnumerable<Authorization>)! )
            {
                authorization.Scopes.Count.Should().Be( 2 );
                count++;
            }

            count.Should().BeGreaterThan( 0 );

            // As scopes are stored as json array, we want to be able to match the presence of scopes in the array
            // not the exact string match of the whole json. Hence scope list is reversed.

            scopes = scopes.Reverse().ToImmutableArray();
            authorizations = authorizationManager.FindAsync( subject, client, status, type, scopes );
            count = 0;
            await foreach( var authorization in (authorizations as IAsyncEnumerable<Authorization>)! )
            {
                authorization.Scopes.Count.Should().Be( scopes.Length );
                count++;
            }

            count.Should().BeGreaterThan( 0 );
        }

        [Test]
        public async Task can_request_a_superset_of_scopes_Async()
        {
            // We actually want to query a superset by providing a subset.
            // Scopes are restrictive

            var authorizationManager =
            DBSetupTestHelper.TestHelper.AutomaticServices.GetRequiredService<IOpenIddictAuthorizationManager>();

            var clientId = nameof( can_request_a_superset_of_scopes_Async );
            var applicationId = (await _applicationTestHelper.ForceCreateApplicationAsync( clientId )).ApplicationId;
            var subject = "aymeric";
            var client = applicationId.ToString();
            var status = "valid";
            var type = "permanent";
            var scopes = new List<string> { "scope1", "scope2", "scope3" }.ToImmutableArray();

            await authorizationManager.CreateAsync
            (
                identity: new ClaimsIdentity(),
                subject: subject,
                client: applicationId.ToString(),
                type: OpenIddictConstants.AuthorizationTypes.Permanent,
                scopes: scopes
            );

            var requestedScopes = new List<string> { "scope1", "scope3" }.ToImmutableArray();

            var authorizations = authorizationManager.FindAsync( subject, client, status, type, requestedScopes );
            var count = 0;
            await foreach( var authorization in (authorizations as IAsyncEnumerable<Authorization>)! )
            {
                authorization.Scopes.Count.Should().Be( scopes.Length );
                count++;
            }

            count.Should().BeGreaterThan( 0 );

        }
    }
}
