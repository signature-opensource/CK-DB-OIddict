using CK.DB.OpenIddictSql.DefaultServer.App;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace CK.DB.OpenIddictSql.DefaultServer.WebHost
{
    public class Program
    {
        public static void Main( string[] args )
        {
            CreateHostBuilder( args ).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder( string[] args ) =>
        Host.CreateDefaultBuilder( args )
            .UseCKMonitoring()
            .ConfigureWebHostDefaults
            (
                webBuilder =>
                {
                    webBuilder
                    .UseScopedHttpContext()
                    .UseStartup<Startup>();
                }
            );
    }
}
