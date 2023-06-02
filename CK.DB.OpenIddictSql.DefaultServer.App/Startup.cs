using System;
using CK.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using CK.DB.AspNet.OpenIddictSql;
using CK.DB.OpenIddictSql.Commands;


namespace CK.DB.OpenIddictSql.DefaultServer.App;

public class Startup
{
    private readonly IActivityMonitor _startupMonitor;

    public Startup( IConfiguration configuration, IWebHostEnvironment env )
    {
        _startupMonitor = new ActivityMonitor
        (
            $"App {env.ApplicationName}/{env.EnvironmentName} on {Environment.MachineName}/{Environment.UserName}."
        );

        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices( IServiceCollection services )
    {
        var connectionString =
        "Server=.;Database=CKOpenIddictDefault;Integrated Security=True;TrustServerCertificate=true";

        services.AddCKDatabase( _startupMonitor, Assembly.GetEntryAssembly()!, connectionString );

        services.AddRouting();
        services.AddOpenIddictAsp();

        // services.AddRazorPages()
        //         .AddRazorPagesOptions
        //         (
        //             options =>
        //             options.RootDirectory = "/Pages"
        //         );

        services.AddCors
        (
            options =>
            {
                options.AddDefaultPolicy
                (
                    x => x
                         .AllowCredentials()
                         .AllowAnyHeader()
                         .AllowAnyMethod()
                         .SetIsOriginAllowed( _ => true )
                );
            }
        );
    }

    public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
    {
        if( env.IsDevelopment() )
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors();

        app.UseRouting();

        app.UseAuthentication();

        app.UseCris();


        app.UseEndpoints
        (
            endpoints =>
            {
                endpoints.MapControllers();
                // endpoints.MapRazorPages();

                // endpoints.MapGet( "/", () => "CK.DB.OpenIddictSql.DefaultServer " );
                endpoints.MapGet
                (
                    "/appinfo",
                    ( DefaultApplication defaultApplication ) => defaultApplication.GetDefaultApplicationInfoAsync()
                );
                endpoints.MapGet
                (
                    "/applications",
                    async ( CommandAdapter<IApplicationsCommand, IApplicationsResult> commandAdapter ) =>
                    {
                        var result = await commandAdapter.HandleAsync( new ActivityMonitor() );

                        return result?.Applications;
                    }
                );
            }
        );

        app.UseSpa( builder => builder.UseProxyToSpaDevelopmentServer( "http://127.0.0.1:8080" ) );


        using( var scope = app.ApplicationServices.CreateScope() )
        {
            var defaultApplication = scope.ServiceProvider.GetRequiredService<DefaultApplication>();
            defaultApplication.EnsureAllDefaultAsync().GetAwaiter().GetResult();
        }
    }
}
