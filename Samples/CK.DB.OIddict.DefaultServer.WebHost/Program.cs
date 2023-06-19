using CK.DB.OIddict.DefaultServer.App;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace CK.DB.OIddict.DefaultServer.WebHost
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
