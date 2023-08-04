# CK-DB-OIddict

| ![logo](ck-db-oiddict_alpha.png) | [![Licence](https://img.shields.io/github/license/signature-opensource/CK-DB-OIddict.svg)](https://github.com/signature-opensource/CK-DB-OIddict/blob/master/LICENSE) | [![AppVeyor](https://ci.appveyor.com/api/projects/status/github/signature-opensource/CK-DB-OIddict?svg=true)](https://ci.appveyor.com/project/Signature-OpenSource/ck-db-OIddict) |
|----------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|

2 libraries built on top of [OpenIddict](https://github.com/openiddict).

| Name                       | Nuget                                                                                                                                    | Description                                                     |
|----------------------------|------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------|
| CK.DB.OIddict        | [![Nuget](https://img.shields.io/nuget/vpre/CK.DB.OIddict.svg)](https://www.nuget.org/packages/CK.DB.OIddict/)               | Stores implementation                                           |
| CK.DB.AspNet.OIddict | [![Nuget](https://img.shields.io/nuget/vpre/CK.DB.AspNet.OIddict.svg)](https://www.nuget.org/packages/CK.DB.AspNet.OIddict/) | AspNet code flow implementation (depend on CK.DB.OIddict) |

CK-DB-OpenIddict is aimed to create an openid connect server with all the backend implementation of the code flow out of the box.
It stores data on a SqlServer database and uses CKSetup to manage it.

You have to handle only what you need to choose, the complexity is already handled.
It is of course configurable but work out of the box. Still, you have to register it into your startup, of course. It is based on [OpenIddict](https://github.com/openiddict) hence register the same way.

I strongly recommend starting with the [samples](Samples).

## Getting started

Implements *oidc code flow* with endpoints and full OpenIddict Server and Validation.

Start with `CK.DB.AspNet.OIddict` package to quickly try it out of the box.
You can follow instructions to start with it: [CK.DB.AspNet.OIddict/README.md](CK.DB.AspNet.OIddict/README.md).

If you encounter any issue, check first the [FAQ](FAQ.md).

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
