# CK-DB-OpenIddict

[![Nuget](https://img.shields.io/nuget/vpre/CK.DB.OpenIddictSql.svg)](https://www.nuget.org/packages/CK.DB.OpenIddictSql/)
[![Licence](https://img.shields.io/github/license/Invenietis/CK-DB-OpenIddictSql.svg)](https://github.com/Invenietis/CK-DB-OpenIddictSql/blob/master/LICENSE)
[![AppVeyor](https://ci.appveyor.com/api/projects/status/github/signature-opensource/CK-DB-OpenIddictSql?svg=true)](https://ci.appveyor.com/project/Signature-OpenSource/ck-db-OpenIddictSql)

## Usage

### CK.DB.OpenIddictSql

Implement Sql Stores and entities on OpenIddict Core.

### CK.DB.AspNet.OpenIddictSql

Implement code flow with endpoints and full OpenIddict Server and Validation.

## About

- CK.DB.OpenIddictSql.DefaultServer.App => An example on how to use the packages with WebFrontAuth.
- SLog.AuthTest => A simple oidc client bound to the DefaultServer.
- CK.DB.OpenIddictSql.DefaultClient => Not used yet. May be used as the SLog.AuthTest, but with OpenIddict client. Why
  not.

About unit tests => I don't think that much relevant. It would take a load of time for no much reason.

## TODO

- Stores identifiers are set by database, set them on instantiation.
- Replace dapper type handling by targeting entities (Application, Token...) instead of built in types (Dictionary<...>) to avoid clashes.
- What about primary identity ? UserId is int, not passed through the claims.
- Add enough Cris commands to provide a way to be able to manage an oidc application management.
- Rewrite dates handling. A workaround is used right now for authorization expiration. The rest is wrong.
- Add consent form example to the default server.
- Map claims
- Startup configuration => for asp package, add a default asp and then a webFrontAuth startup.
- Implement missing / incomplete stores => For example, handle multiple scopes. Mainly, be able to query json.
