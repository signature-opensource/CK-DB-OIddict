using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using CK.DB.OIddict.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenIddict.Abstractions;
using static CK.Testing.DBSetupTestHelper;

namespace CK.DB.OIddict.Tests
{
    public class ApplicationStoreTests
    {
        private readonly ApplicationTestHelper _applicationTestHelper = new ApplicationTestHelper();

        [Test]
        public async Task should_create_new_application_Async()
        {
            var clientId = "ckdb-default-app";
            await _applicationTestHelper.ForceCreateApplicationAsync( clientId );
        }

        [Test]
        public async Task can_find_application_by_redirect_uri_Async()
        {
            var app = await _applicationTestHelper.ForceCreateApplicationAsync
            (
                nameof( can_find_application_by_redirect_uri_Async )
            );

            var appStore = TestHelper.AutomaticServices.GetRequiredService<IOpenIddictApplicationStore<Application>>();

            await appStore.SetRedirectUrisAsync
            (
                app,
                new List<string>() { "https://whatever.com" }.ToImmutableArray(),
                new CancellationToken()
            );
            await appStore.UpdateAsync( app, new CancellationToken() );
            var result = appStore.FindByRedirectUriAsync( "https://whatever.com", new CancellationToken() );
            var count = 0;
            await foreach( var application in result )
            {
                count++;
                application.ClientId.Should().Be( nameof( can_find_application_by_redirect_uri_Async ) );
            }

            count.Should().BeGreaterThan( 0 );
        }
    }
}
