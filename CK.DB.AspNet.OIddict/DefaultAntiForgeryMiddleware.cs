using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace CK.DB.AspNet.OIddict
{
    public class DefaultAntiForgeryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAntiforgery _antiForgery;
        private readonly string _cookieName;

        public DefaultAntiForgeryMiddleware
        (
            RequestDelegate next,
            IAntiforgery antiForgery,
            string cookieName
        )
        {
            _next = next;
            _antiForgery = antiForgery;
            _cookieName = cookieName;
        }

        [SuppressMessage( "Style", "VSTHRD200:Use \"Async\" suffix for async methods" )]
        public Task Invoke( HttpContext context )
        {
            var tokens = _antiForgery.GetAndStoreTokens( context );

            if( tokens.RequestToken != null )
            {
                context.Response.Cookies.Append
                (
                    _cookieName,
                    tokens.RequestToken,
                    new CookieOptions
                    {
                        HttpOnly = false, // let the front-end read this cookie to set a form or header value.
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                    }
                );
            }

            return _next( context );
        }
    }

    public static class DefaultAntiForgeryMiddlewareExtensions
    {
        public static IApplicationBuilder UseDefaultAntiForgeryMiddleware
        (
            this IApplicationBuilder app,
            string cookieName = "AntiForgeryCookie"
        )
            => app.UseMiddleware<DefaultAntiForgeryMiddleware>( cookieName );
    }
}
