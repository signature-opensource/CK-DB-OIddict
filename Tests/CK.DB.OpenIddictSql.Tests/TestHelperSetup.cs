using System.IO;
using CK.Core;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using static CK.Testing.DBSetupTestHelper;

namespace CK.DB.OpenIddictSql.Tests
{
    [SetUpFixture]
    public class TestHelperSetup
    {
        [OneTimeSetUp]
        public void EnsureAutomaticServices()
        {
            // Creates a configuration object once for all.
            var configuration = new ConfigurationBuilder()
                                .SetBasePath( Directory.GetCurrentDirectory() )
                                .AddJsonFile( "appsettings.json", optional: true )
                                .Build();
            // Each time a new AutomaticServices is created we make the configuration from the appsettings available
            // to any services configuration that may need it.
            TestHelper.AutomaticServicesConfiguring += ( o, e ) =>
            {
                TestHelper.Monitor.Trace( $"Service configuration can use the Configuration object." );
                e.ServiceRegister.StartupServices.Add<IConfiguration>( configuration );

                var managerBuilder = e.ServiceRegister.Services.AddOpenIddictSql();
                e.ServiceRegister.StartupServices.Add( managerBuilder );
            };
        }
    }
}
