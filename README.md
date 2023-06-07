# CK-DB-OpenIddict

| ![logo](ck-db-openiddictsql_alpha.png) | [![Licence](https://img.shields.io/github/license/signature-opensource/CK-DB-OpenIddictSql.svg)](https://github.com/signature-opensource/CK-DB-OpenIddictSql/blob/master/LICENSE) | [![AppVeyor](https://ci.appveyor.com/api/projects/status/github/signature-opensource/CK-DB-OpenIddictSql?svg=true)](https://ci.appveyor.com/project/Signature-OpenSource/ck-db-OpenIddictSql) |
|----------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|

2 libraries built on top of [OpenIddict](https://github.com/openiddict).

| Name                       | Nuget                                                                                                                                    | Description                                                     |
|----------------------------|------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------|
| CK.DB.OpenIddictSql        | [![Nuget](https://img.shields.io/nuget/vpre/CK.DB.OpenIddictSql.svg)](https://www.nuget.org/packages/CK.DB.OpenIddictSql/)               | Stores implementation                                           |
| CK.DB.AspNet.OpenIddictSql | [![Nuget](https://img.shields.io/nuget/vpre/CK.DB.AspNet.OpenIddictSql.svg)](https://www.nuget.org/packages/CK.DB.AspNet.OpenIddictSql/) | AspNet code flow implementation (depend on CK.DB.OpenIddictSql) |

Start with the AspNet package to quickly try it out of the box.

## Usage

### CK.DB.AspNet.OpenIddictSql

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

### CK.DB.OpenIddictSql

Implement Sql Stores and entities on OpenIddict Core.

Use this package to have an sql implementation with CK Database. You can then follow
the [Samples](https://github.com/openiddict/openiddict-samples) from OpenIddict. You can consider this package as a
replacement of the EntityFramework one.

```csharp
// The stores are based on CK Database
var connectionString = "Server=.;Database=CKOpenIddictDefault;Integrated Security=True;TrustServerCertificate=true";
services.AddCKDatabase( new ActivityMonitor(), Assembly.GetEntryAssembly()!, connectionString );

// Add OpenIddict code flow like in samples
services.AddOpenIddict()
        .AddCore
        (
            builder =>
            {
                // Configure OpenIddict to use the CK Database stores and models.
                builder.UseOpenIddictCoreSql();
            }
        )
        .AddServer
        (
            builder =>
            {
                // see OpenIddict samples
            }
        )
        .AddValidation
        (
            builder =>
            {
                // see OpenIddict samples
            }
        );
```

And that is it.

### Create an administration panel with Cris

Using [Cris](CK.DB.OpenIddictSql/Cris/), you can build a front end to manage applications or else to create the best oidc provider !

## About

- CK.DB.OpenIddictSql.DefaultServer.App => An example on how to use the packages with WebFrontAuth.
- SLog.AuthTest => A simple oidc client bound to the DefaultServer.
- CK.DB.OpenIddictSql.DefaultClient => Not used yet. May be used as the SLog.AuthTest, but with OpenIddict client. Why
  not.

About unit tests => I don't think that much relevant. It would take a load of time for no much reason.

## TODO

- Stores identifiers are set by database, set them on instantiation.
- Replace dapper type handling by targeting entities (Application, Token...) instead of built in types (Dictionary<...>)
  to avoid clashes.
- What about primary identity ? UserId is int, not passed through the claims.
- Add enough Cris commands to provide a way to be able to manage an oidc application management.
- Rewrite dates handling. A workaround is used right now for authorization expiration. The rest is wrong.
- Add consent form example to the default server.
- Map claims
- Startup configuration => for asp package, add a default asp and then a webFrontAuth startup.
- Implement missing / incomplete stores => For example, handle multiple scopes. Mainly, be able to query json.
