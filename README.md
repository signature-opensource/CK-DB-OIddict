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

services.AddOpenIddictAspWebFrontAuth
(
    "/", // Your login path
    serverBuilder: server => server.AddDevelopmentEncryptionCertificate()
                                   .AddDevelopmentSigningCertificate()
);
```

Then you need an actual front end, check out the sample [in our example server](Samples/CK.DB.OIddict.DefaultServer.App/WebFrontAuth) that you can copy paste.
The `loginPath` is mapped to `"/"`;

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

About unit tests => I don't think that much relevant. It would take a load of time for no much reason.

## TODO

- Replace dapper type handling by targeting entities (Application, Token...) instead of built in types (Dictionary<...>)
  to avoid clashes.
- What about primary identity ? UserId is int, not passed through the claims.
- Add enough Cris commands to provide a way to be able to manage an oidc application management.
- Rewrite dates handling. A workaround is used right now for authorization expiration. The rest is wrong.
- Add consent form example to the default server.
- Map claims
- Make most of classes internal
- Simplify README.md by showing up directly the startup with OpenIddict config exposed.
