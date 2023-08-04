# CK.DB.OIddict

## Getting started

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

## Application

Of course, you need an application. There are [Cris commands](Cris/ApplicationHandler.cs) that you can
call directly from your frontend app. You can also do a quick and dirty solution like in
the [sample](../Samples/CK.DB.OIddict.DefaultServer.App/Startup.cs).
They are some examples on how to create, read, update or delete an application.

Here is an example to quickly setup an application from the C# code:

```csharp
// Inject IOpenIddictApplicationManager into _applicationManager

var appDescriptor = new ApplicationDescriptorBuilder
                    (
                        "ckdb-default-app",
                        "901564A5-E7FE-42CB-B10D-61EF6A8F3654"
                    )
                    .WithDisplayName( "CK-DB Default application" )
                    .EnsureCodeDefaults()
                    .AddRedirectUri( new Uri( "https://localhost:5044/signin-oidc" ) )
                    .Build();

if( await _applicationManager.FindByClientIdAsync( appDescriptor.ClientId! ) == null )
    await _applicationManager.CreateAsync( appDescriptor );
```

## About technical implementation and choices

For developers or this library

The core / main logic is handled by [Stores](OpenIddict/Stores). They are internally called by OpenIddict managers.
Here they provide a way to access sql database created from [Db](Db).
They are also indirectly called from [Cris](Cris) commands and queries.

