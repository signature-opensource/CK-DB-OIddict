using Microsoft.AspNetCore.Authentication.Cookies;

namespace SLog.AuthTest
{
    public class Program
    {
        public static void Main( string[] args )
        {
            var builder = WebApplication.CreateBuilder( args );

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddRazorPages();

            // Local sign-in is done using cookies
            builder.Services.AddAuthentication( CookieAuthenticationDefaults.AuthenticationScheme )
                   .AddOpenIdConnect
                   (
                       oidc =>
                       {
                           // Hi! We're configuring a website - a Service Provider, or SP - that wants to use another website - an Identity Provider, or IdP - to login people.
                           // This SP doesn't have a User database. In fact, we don't have a database! We still want to login people though.

                           // The local scheme to login with Oidc is OpenIdConnectDefaults.AuthenticationScheme.

                           // Here, SP = the site you're configuring, and IdP = Azure.
                           // This URL is provided by the IdP, and contains basically all supported claims, auth. types, and various URLs for e.g. tokens
                           oidc.MetadataAddress = "https://localhost:44337/.well-known/openid-configuration";
                           oidc.ResponseType = "code";
                           // This is the client ID, or application ID, and its client secret. This authenticates this SP against the IdP,
                           // and verifies that we are an application that is allowed to login with it.
                           oidc.ClientId = "ckdb-default-app";
                           oidc.ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654";
                           // oidc.Scope.Clear();
                           oidc.Scope.Add( "openid" );
                           // Add this scope if there is no db. The user identifier will be filled with authentication information.
                           // oidc.Scope.Add( "authinfo" );
                           // This is the route used in the IdP's "Reply URL" (e.g. "https://localhost:5044/signin-oidc")
                           oidc.CallbackPath = "/signin-oidc";
                           oidc.Events.OnTokenResponseReceived = context => Task.CompletedTask;
                       }
                   )
                   .AddCookie
                   (
                       options => options.Cookie.Name = "SLogCookie"
                   ); // Add cookies to stay logged in, this is what signs the user in locally

            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if( !app.Environment.IsDevelopment() )
            {
                app.UseExceptionHandler( "/Error" );
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapRazorPages();

            app.Run();
        }
    }
}
