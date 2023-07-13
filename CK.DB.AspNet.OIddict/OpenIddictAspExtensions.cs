using System;
using System.Reflection;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CK.DB.AspNet.OIddict
{
    public static class OpenIddictAspExtensions
    {
        /// <summary>
        /// Add Authorization Code Flow and its endpoints.
        /// </summary>
        /// <param name="builder">The configuration delegate used to configure the server services.</param>
        /// <param name="authenticationScheme">The scheme that handle the Challenge.</param>
        /// <param name="loginPath">If set, bypass Authentication Handler on Challenge on the provided scheme.
        /// Use it if the authenticationScheme does not support standard Challenge.</param>
        /// <param name="consentPath">Must be provided when using a flow that require user consent.</param>
        /// <returns></returns>
        public static OpenIddictServerBuilder UseOpenIddictServerAsp
        (
            this OpenIddictServerBuilder builder,
            string authenticationScheme,
            string? loginPath = null,
            string? consentPath = null
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
                    loginPath,
                    consentPath
                )
            );

            services.AddControllersWithViews() //TODO: remove views and register antiforgery
                    .AddApplicationPart( Assembly.Load( Assembly.GetExecutingAssembly().GetName().Name! ) );

            services.AddRouting();


            return builder;
        }

        public static OpenIddictServerBuilder WithDefaultAntiForgery
        (
            this OpenIddictServerBuilder builder,
            Action<AntiforgeryOptions>? antiForgeryOptions = null
        )
        {
            builder.Services.AddAntiforgery
            (
                options =>
                {
                    options.HeaderName = "X-CSRF-TOKEN";
                    options.FormFieldName = "__RequestVerificationToken";
                    options.Cookie.Name = ".asp.AntiForgeryCookie";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                    antiForgeryOptions?.Invoke( options );
                }
            );

            return builder;
        }
    }
}
