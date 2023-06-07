There are 2 main ideas here :

1. Support Open Iddict code flow endpoints
2. Support custom authentication handlers like WebFrontAuth

Configure endpoints paths : [CK.DB.AspNet.OpenIddictSql.Constants](Constants.cs)

There is one trick that is important to point out. With `LoginPath` [CK.DB.AspNet.OpenIddictSql.Configuration](Configuration.cs) set, the actual login will be called with `Redirect` instead of the usual `Challenge`. Hence the redirection is crafted directly.

There is the possibility to call [CK.DB.AspNet.OpenIddictSql.OpenIddictAspExtensions.AddOpenIddictAspWebFrontAuth](OpenIddictAspExtensions.cs) to setup the services with WebFrontAuth. Note that the `LoginPath` is then mandatory since WFA does not support `Challenge`.
