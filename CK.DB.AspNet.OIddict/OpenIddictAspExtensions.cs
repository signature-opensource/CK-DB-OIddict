using System;
using System.Reflection;
using CK.AspNet.Auth;
using CK.DB.OIddict;
using Microsoft.Extensions.DependencyInjection;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace CK.DB.AspNet.OIddict
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
        /// <param name="services">The services collection.</param>
        /// <param name="loginPath">Path to WFA login page where to redirect to on Challenge.</param>
        /// <param name="wfaOptions">Configuration action</param>
        /// <param name="serverBuilder">The configuration delegate used to configure OpenIddict server services.</param>
        /// <param name="coreBuilder">The configuration delegate used to configure the OpenIddict services.</param>
        /// <param name="validationBuilder">The configuration delegate used to configure OpenIddict validation services.</param>
        /// <returns></returns>
        public static IServiceCollection AddOpenIddictAspWebFrontAuth
        (
            this IServiceCollection services,
            string loginPath,
            Action<WebFrontAuthOptions>? wfaOptions = null,
            Action<OpenIddictServerBuilder>? serverBuilder = null,
            Action<OpenIddictCoreBuilder>? coreBuilder = null,
            Action<OpenIddictValidationBuilder>? validationBuilder = null
        )
        {
            services.AddAuthentication( WebFrontAuthOptions.OnlyAuthenticationScheme )
                    .AddWebFrontAuth
                    (
                        options =>
                        {
                            options.AuthCookieName = ".oidcServerWebFront";
                            //TODO: Let's see if AuthenticationCookieMode can be set to default.
                            options.CookieMode = AuthenticationCookieMode.RootPath;

                            wfaOptions?.Invoke( options );
                        }
                    );

            services.AddOpenIddict()
                    .AddCore
                    (
                        builder =>
                        {
                            builder.UseOpenIddictCoreSql();
                            coreBuilder?.Invoke( builder );
                        }
                    )
                    .AddServer
                    (
                        builder =>
                        {
                            builder.UseOpenIddictServerAsp
                            (
                                WebFrontAuthOptions.OnlyAuthenticationScheme,
                                loginPath
                            );

                            builder.RegisterScopes( Scopes.Email, Scopes.Profile, Scopes.Roles, Scopes.OpenId, "authinfo" );
                            builder.RegisterClaims( Claims.Name, Claims.Email, Claims.Profile );

                            serverBuilder?.Invoke( builder );
                        }
                    )
                    .AddValidation
                    (
                        builder =>
                        {
                            builder.UseLocalServer();

                            builder.UseAspNetCore();

                            validationBuilder?.Invoke( builder );
                        }
                    );

            return services;
        }

        /// <summary>
        /// Add Authorization Code Flow and its endpoints.
        /// </summary>
        /// <param name="builder">The configuration delegate used to configure the server services.</param>
        /// <param name="authenticationScheme">The scheme that handle the Challenge.</param>
        /// <param name="loginPath">If set, bypass Authentication Handler on Challenge on the provided scheme.
        /// Use it if the authenticationScheme does not support standard Challenge.</param>
        /// <returns></returns>
        public static OpenIddictServerBuilder UseOpenIddictServerAsp
        (
            this OpenIddictServerBuilder builder,
            string authenticationScheme,
            string? loginPath = null
        )
        {
            builder.SetAuthorizationEndpointUris( Constants.AuthorizeUri )
                   .SetLogoutEndpointUris( Constants.LogoutUri )
                   .SetTokenEndpointUris( Constants.TokenUri )
                   .SetUserinfoEndpointUris( Constants.UserInfoUri );

            builder.AllowAuthorizationCodeFlow();
            // builder.AllowRefreshTokenFlow();

            builder.UseAspNetCore()
                   .EnableAuthorizationEndpointPassthrough()
                   .EnableLogoutEndpointPassthrough()
                   .EnableTokenEndpointPassthrough()
                   .EnableUserinfoEndpointPassthrough()
                   .EnableStatusCodePagesIntegration();

            var services = builder.Services;

            services.AddSingleton
            (
                new Configuration
                (
                    authenticationScheme,
                    loginPath
                )
            );

            services.AddControllers()
                    .AddApplicationPart( Assembly.Load( Assembly.GetExecutingAssembly().GetName().Name! ) );


            return builder;
        }
    }
}
