using System.Reflection;
using CK.AspNet.Auth;
using CK.DB.OpenIddictSql;
using Microsoft.Extensions.DependencyInjection;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace CK.DB.AspNet.OpenIddictSql
{
    public static class OpenIddictAspExtensions
    {
        /// <summary>
        /// Add OpenIddict Core and registers the Sql stores services in the DI container and
        /// configures OpenIddict to use the related Sql entities by default.
        /// Add OpenIddict Server and registers OpenIddict with Authorization Code Flow and its endpoints.
        /// Add OpenIddict Validation.
        /// Add WebFrontAuth Authentication for login.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddOpenIddictAsp( this IServiceCollection services )
        {
            var authenticationScheme = WebFrontAuthOptions.OnlyAuthenticationScheme;
            var loginPath = "/";

            services.AddSingleton<Configuration>
            (
                new Configuration
                (
                    authenticationScheme,
                    loginPath
                )
            );

            services.AddControllers()
                    .AddApplicationPart( Assembly.Load( Assembly.GetExecutingAssembly().GetName().Name! ) );

            services.AddAuthentication( WebFrontAuthOptions.OnlyAuthenticationScheme )
                    .AddWebFrontAuth
                    (
                        options =>
                        {
                            //TODO: Let's see if AuthenticationCookieMode can be set to default.
                            options.CookieMode = AuthenticationCookieMode.RootPath;
                            options.AuthCookieName = ".oidcServerWebFront";
                        }
                    );

            services.AddOpenIddictSql();

            services.AddOpenIddict()
                    .AddServer
                    (
                        options =>
                        {
                            options.SetAuthorizationEndpointUris( ConstantsConfiguration.AuthorizeUri )
                                   .SetLogoutEndpointUris( ConstantsConfiguration.LogoutUri )
                                   .SetTokenEndpointUris( ConstantsConfiguration.TokenUri )
                                   .SetUserinfoEndpointUris( ConstantsConfiguration.UserInfoUri );

                            options.RegisterScopes( Scopes.Email, Scopes.Profile, Scopes.Roles, Scopes.OpenId );


                            options.AllowAuthorizationCodeFlow();
                            // options.AllowRefreshTokenFlow();

                            options.AddDevelopmentEncryptionCertificate()
                                   .AddDevelopmentSigningCertificate();

                            options.UseAspNetCore()
                                   .EnableAuthorizationEndpointPassthrough()
                                   .EnableLogoutEndpointPassthrough()
                                   .EnableTokenEndpointPassthrough()
                                   .EnableUserinfoEndpointPassthrough()
                                   .EnableStatusCodePagesIntegration();

                            options.RegisterClaims( Claims.Name, Claims.Email, Claims.Profile );
                        }
                    )
                    .AddValidation
                    (
                        options =>
                        {
                            options.UseLocalServer();

                            options.UseAspNetCore();
                        }
                    );

            return services;
        }
    }
}
