using System.Reflection;
using CK.AspNet.Auth;
using CK.DB.OpenIddictSql;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
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
            services.AddControllers()
                    .AddApplicationPart( Assembly.Load( Assembly.GetExecutingAssembly().GetName().Name! ) );

            services.AddAuthentication( WebFrontAuthOptions.OnlyAuthenticationScheme )
                    // This should not create a cookie, it is a workaround to get a redirection.
                    // By calling ChallengeAsync with WFA schema, you will be redirected to the login path.
                    .AddCookie
                    (
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        options =>
                        {
                            options.LoginPath = new PathString( "/" );
                            // You should not see this cookie:
                            options.Cookie.Name = "OpenIddictServer";
                        }
                    )
                    .AddWebFrontAuth
                    (
                        options =>
                        {
                            options.CookieMode = AuthenticationCookieMode.RootPath;
                            options.ForwardChallenge = CookieAuthenticationDefaults.AuthenticationScheme;
                        }
                    );

            services.AddOpenIddictSql();

            services.AddOpenIddict()
                    .AddServer
                    (
                        options =>
                        {
                            options.SetAuthorizationEndpointUris( "connect/authorize" )
                                   .SetLogoutEndpointUris( "connect/logout" )
                                   .SetTokenEndpointUris( "connect/token" )
                                   .SetUserinfoEndpointUris( "connect/userinfo" );

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
