﻿using System;
using CK.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using CK.AspNet.Auth;
using CK.DB.AspNet.OIddict;
using CK.DB.OIddict.Commands;
using Microsoft.AspNetCore.Antiforgery;
using static OpenIddict.Abstractions.OpenIddictConstants;


namespace CK.DB.OIddict.DefaultServer.App
{
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
            "Server=.;Database=CKOIddictDefault;Integrated Security=True;TrustServerCertificate=true";

            services.AddCKDatabase( _startupMonitor, Assembly.GetEntryAssembly()!, connectionString );


            services.AddAuthentication( WebFrontAuthOptions.OnlyAuthenticationScheme )
                    .AddWebFrontAuth
                    (
                        options =>
                        {
                            options.CookieMode = AuthenticationCookieMode.RootPath;
                            options.AuthCookieName = ".oidcServerWebFront";
                        }
                    );

            services.AddOpenIddict()
                    .AddCore( builder => builder.UseOIddictCoreSql() )
                    .AddServer
                    (
                        builder =>
                        {
                            builder.UseOIddictServerAsp
                                   (
                                       WebFrontAuthOptions.OnlyAuthenticationScheme,
                                       "/",
                                       "/Authorization/Consent.html"
                                   )
                                   .WithDefaultAntiForgery( o => o.FormFieldName = "__RequestVerificationToken" );

                            builder.AddDevelopmentEncryptionCertificate()
                                   .AddDevelopmentSigningCertificate();

                            builder.RegisterScopes
                            (
                                Scopes.Email,
                                Scopes.Profile,
                                Scopes.Roles,
                                Scopes.OpenId,
                                "authinfo"
                            );

                            builder.RegisterClaims( Claims.Name, Claims.Email, Claims.Profile );
                        }
                    )
                    .AddValidation
                    (
                        builder =>
                        {
                            builder.UseLocalServer();

                            builder.UseAspNetCore();
                        }
                    );

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

        public void Configure( IApplicationBuilder app, IWebHostEnvironment env, IAntiforgery antiForgery )
        {
            if( env.IsDevelopment() )
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseDefaultAntiForgeryMiddleware( "AntiForgeryCookie" );

            app.UseCris();

            app.UseEndpoints
            (
                endpoints =>
                {
                    endpoints.MapControllers();
                    // endpoints.MapRazorPages();

                    endpoints.MapGet
                    (
                        "getApp",
                        async
                        (
                            string clientId,
                            CommandAdapter<IGetApplicationCommand, IApplicationPoco> getCommandAdapter
                        ) =>
                        {
                            return await getCommandAdapter.HandleAsync
                            (
                                new ActivityMonitor(),
                                i => i.ClientId = clientId
                            );
                        }
                    );
                    endpoints.MapGet
                    (
                        $"update",
                        async
                        (
                            string? clientId,
                            CommandAdapter<IUpdateApplicationCommand, ISimpleCrisResult> updateCommandAdapter,
                            CommandAdapter<IGetApplicationCommand, IApplicationPoco> getCommandAdapter
                        ) =>
                        {
                            clientId ??= "ckdb-default-app";
                            var pocoGetAppResult = await getCommandAdapter.HandleAsync
                            (
                                new ActivityMonitor(),
                                i => i.ClientId = clientId
                            );

                            pocoGetAppResult!.DisplayName += " updated";

                            var updateResult = await updateCommandAdapter.HandleAsync
                            (
                                new ActivityMonitor(),
                                i => i.ApplicationPoco = pocoGetAppResult
                            );

                            return updateResult;
                        }
                    );
                    endpoints.MapGet
                    (
                        "/applications",
                        async ( CommandAdapter<IGetApplicationsCommand, IGetApplicationsResult> commandAdapter ) =>
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
}
