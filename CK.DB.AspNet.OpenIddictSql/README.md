# CK.DB.AspNet.OpenIddictSql

## Getting started

Implements oidc code flow with endpoints and full OpenIddict Server and Validation.

**WebFrontAuth Quick start.**

First, configure OpenIddict and WebFrontAuth services:

```csharp
var connectionString = "Server=.;Database=CKOpenIddictDefault;Integrated Security=True;TrustServerCertificate=true";
services.AddCKDatabase( new ActivityMonitor(), Assembly.GetEntryAssembly()!, connectionString );

services.AddOpenIddictAspWebFrontAuth
(
    "/", // Your login path
    serverBuilder: server => server.AddDevelopmentEncryptionCertificate()
                                   .AddDevelopmentSigningCertificate()
);
```

Then you need an actual front end, you can
use [WebFrontAuth sample](https://github.com/Woinkk/CK-Sample-WebFrontAuth/tree/master/WFATester), hence the `loginPath`
mapped to `"/"`.
You can also directly look
at [the same sample adapted for the flow in our example server](CK.DB.OpenIddictSql.DefaultServer.App/WebFrontAuth).

Of course, you need an application :

```csharp
// using static OpenIddict.Abstractions.OpenIddictConstants.ConsentTypes;
// using static OpenIddict.Abstractions.OpenIddictConstants.Permissions;
// using static OpenIddict.Abstractions.OpenIddictConstants.Requirements;
// Inject IOpenIddictApplicationManager into _applicationManager

await _applicationManager.CreateAsync
(
    new OpenIddictApplicationDescriptor
    {
        ClientId = "ckdb-default-app",
        ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",
        ConsentType = Explicit,
        DisplayName = "CK-DB Default application",
        RedirectUris =
        {
            new Uri( "https://oidcdebugger.com/debug" ),
            new Uri( "https://localhost:5044/signin-oidc" ),
        },
        PostLogoutRedirectUris =
        {
            new Uri( "https://localhost:7273/callback/logout/local" ),
        },
        Permissions =
        {
            Endpoints.Authorization,
            Endpoints.Logout,
            Endpoints.Token,
            GrantTypes.AuthorizationCode,
            ResponseTypes.Code,
            Scopes.Email,
            Scopes.Profile,
            Scopes.Roles,
        },
        Requirements =
        {
            Features.ProofKeyForCodeExchange,
        },
    }
);
```

Go ahead and try the flow. You can use the [client example](SLog.AuthTest)
or [OpenID Connect \<debugger\/\>](https://oidcdebugger.com).

**More granular configuration**

This is the recommended startup, but may be verbose to begin with and the previous sample still is able to behave the
same.

This sample is the one to use if you don't want to use WebFrontAuth. Here I am still using WebFrontAuth but you can
change the authentication scheme to fit your needs. Note that you also want to remove the `loginPath` if not necessary.

```csharp
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
        .AddCore( builder => builder.UseOpenIddictCoreSql() )
        .AddServer
        (
            builder =>
            {
                builder.UseOpenIddictServerAsp( WebFrontAuthOptions.OnlyAuthenticationScheme, "/" );

                builder.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();
                builder.RegisterScopes( Scopes.Email, Scopes.Profile, Scopes.Roles, Scopes.OpenId );
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
```

Same as previous, create a front end and an application.


## About technical implementation and choices

There are 2 main ideas here :

1. Support Open Iddict code flow endpoints
2. Support custom authentication handlers like WebFrontAuth

Configure endpoints paths : [CK.DB.AspNet.OpenIddictSql.Constants](Constants.cs)

There is one trick that is important to point out. With `LoginPath` from [CK.DB.AspNet.OpenIddictSql.Configuration](Configuration.cs) set, the actual login will be called with `Redirect` instead of the usual `Challenge`. Hence the redirection is crafted directly.

There is the possibility to call [CK.DB.AspNet.OpenIddictSql.OpenIddictAspExtensions.AddOpenIddictAspWebFrontAuth](OpenIddictAspExtensions.cs) to setup the services with WebFrontAuth. Note that the `LoginPath` is then mandatory since WFA does not support `Challenge`.
