# CK-DB-OIddict

| ![logo](ck-db-oiddict_alpha.png) | [![Licence](https://img.shields.io/github/license/signature-opensource/CK-DB-OIddict.svg)](https://github.com/signature-opensource/CK-DB-OIddict/blob/master/LICENSE) | [![AppVeyor](https://ci.appveyor.com/api/projects/status/github/signature-opensource/CK-DB-OIddict?svg=true)](https://ci.appveyor.com/project/Signature-OpenSource/ck-db-OIddict) |
|----------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|

2 libraries built on top of [OpenIddict](https://github.com/openiddict).

| Name                       | Nuget                                                                                                                                    | Description                                                     |
|----------------------------|------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------|
| CK.DB.OIddict        | [![Nuget](https://img.shields.io/nuget/vpre/CK.DB.OIddict.svg)](https://www.nuget.org/packages/CK.DB.OIddict/)               | Stores implementation                                           |
| CK.DB.AspNet.OIddict | [![Nuget](https://img.shields.io/nuget/vpre/CK.DB.AspNet.OIddict.svg)](https://www.nuget.org/packages/CK.DB.AspNet.OIddict/) | AspNet code flow implementation (depend on CK.DB.OIddict) |

Start with the AspNet package to quickly try it out of the box.

It is recommended to give a try and follow the **Getting started** section right below.
If you already are familiar with OpenIddict, or want to go a little bit deeper, skip this section and read **About** section or directly [CK.DB.AspNet.OIddict/README.md](CK.DB.AspNet.OIddict/README.md).

## Getting started

Implements *oidc code flow* with endpoints and full OpenIddict Server and Validation.

**WebFrontAuth Quick start.**

First, configure OpenIddict and WebFrontAuth services:

```csharp
var connectionString = "Server=.;Database=CKOpenIddictDefault;Integrated Security=True;TrustServerCertificate=true";
services.AddCKDatabase( new ActivityMonitor(), Assembly.GetEntryAssembly()!, connectionString );

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

services.AddOpenIddict()
        .AddCore( builder => builder.UseOpenIddictCoreSql() )
        .AddServer
        (
            builder =>
            {
                builder.UseOpenIddictServerAsp
                (
                    WebFrontAuthOptions.OnlyAuthenticationScheme,
                    "/",
                    "/Authorization/Consent.html"
                );

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
```

Then you need an actual front end, check out the sample [in our example server](Samples/CK.DB.OIddict.DefaultServer.App/WebFrontAuth) that you can copy paste.

- The `loginPath` is mapped to `"/"`.
- The `consentPath` is mapped to `"/Authorization/Consent.html"`

The consent page is called from the backend with all values sent as a query string. Those values has to be sent back with the form.
The AntiForgery Token has to be send in the form too.

To finish, [create an oidc application](./CK.DB.AspNet.OIddict/README.md) that fits your needs.

### Quick test

Check out [OpenIddict Sample](https://github.com/openiddict/openiddict-samples/blob/dev/samples/Velusia/Velusia.Server/Worker.cs) to create an application.

Go ahead and try the flow. You can use the [client example](Samples/SLog.AuthTest)
or [OpenID Connect \<debugger\/\>](https://oidcdebugger.com) for example.

### Create an administration panel with Cris

Using [Cris](CK.DB.OIddict/Cris), you can build a front end to manage applications or else to create the best
oidc provider !

## About

- [CK.DB.AspNet.OIddict](CK.DB.AspNet.OIddict) => Implements oidc code flow with endpoints and full OpenIddict Server and Validation.
- [CK.DB.OIddict](CK.DB.OIddict) => Implement Sql Stores and entities on OpenIddict Core.
- [Samples/CK.DB.OIddict.DefaultServer.App](Samples/CK.DB.OIddict.DefaultServer.App) => An example on how to use the packages with WebFrontAuth.
- [SLog.AuthTest](Samples/SLog.AuthTest) => A simple oidc client bound to the DefaultServer.
- [CK.DB.OIddict.DefaultClient](Samples/CK.DB.OIddict.DefaultClient) => Not used yet. May be used as the SLog.AuthTest, but with OpenIddict client. Why
  not.

## TODO

- Add enough Cris commands to provide a way to be able to manage an oidc application management.
- Make most of classes internal
