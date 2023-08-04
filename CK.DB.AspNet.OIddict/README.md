# CK.DB.AspNet.OIddict

## Usage

Implements **oidc code flow** with endpoints and full OpenIddict Server and Validation.

I strongly recommend starting with the [samples](../Samples).

### Startup and configuration

**WebFrontAuth as example.**

First, configure OpenIddict and WebFrontAuth services:

```csharp
var connectionString = "Server=.;Database=CKOpenIddictDefault;Integrated Security=True;TrustServerCertificate=true";
services.AddCKDatabase( new ActivityMonitor(), Assembly.GetEntryAssembly()!, connectionString );

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
                builder.UseOpenIddictServerAsp
                       (
                           WebFrontAuthOptions.OnlyAuthenticationScheme,
                           "/",
                           "/Authorization/Consent.html"
                       )
                       .WithDefaultAntiForgery( o => o.FormFieldName = "__RequestVerificationToken" );

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

On the Configure method, add the default AntiForgery middleware between auth and endpoints.

```csharp
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultAntiForgeryMiddleware( "AntiForgeryCookie" );

app.UseEndpoints();
```

### Identity

Next, you want to [implement `IIdentityStrategy`](./Identity/IIdentityStrategy.cs) to validate the user (against a database usually) and map the claims. This will be automatically injected to the dependency injection container.
Here is a simple implementation for [WebFrontAuth](../Samples/CK.DB.OIddict.DefaultServer.App/WfaIdentityStrategy.cs).

### Login and Consent pages

Then you need an actual front end, check out the
sample [in our example server](../Samples/CK.DB.OIddict.DefaultServer.App/WebFrontAuth) that you can copy paste.

If you have used the default from this README of the sample, consider this:

- The `loginPath` is mapped to `"/"`.
- The `consentPath` is mapped to `"/Authorization/Consent.html"`
- You have to handle the AntiForgery by getting the value of the cookie `AntiForgeryCookie` and put it into a form value
  with name `__RequestVerificationToken`.

#### Login

This is actually a Challenge type of Login.
Follow these steps:

- Login the user and create a cookie as a standard login.
- Redirect the user based on the `ReturnUrl` value from the query string.

#### Consent

The consent page is called from the backend with all values sent as a query string. Those values has to be sent back
with the form.
The AntiForgery Token has to be send in the form too.

Follow these steps:

- Create a form with 2 buttons : _Accept_ or _Deny_.
- A click on _Accept_ will add a form input with name `submit.Accept`, value does not matter.
- A click on _Deny_ will add a form input with name `submit.Deny`, value does not matter.
- Add all values from the query string to this form as hidden input.
- Get the cookie value of `AntiForgeryCookie`.
- Add the value of the cookie to the form as hidden input, with name `__RequestVerificationToken`.

### Application

Of course, you need an application. There are [Cris commands](../CK.DB.OIddict/Cris/ApplicationHandler.cs) that you can call directly from your frontend app. You can also do a quick and dirty solution like in the [sample](../Samples/CK.DB.OIddict.DefaultServer.App/Startup.cs).
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

### Tests

Go ahead and try the flow. You can use the [client example](../Samples/SLog.AuthTest)
or [OpenID Connect \<debugger\/\>](https://oidcdebugger.com).

## About technical implementation and choices

There are 2 main ideas here :

1. Support Open Iddict code flow endpoints
2. Support custom authentication handlers like WebFrontAuth

Configure endpoints paths : [CK.DB.AspNet.OIddict.Constants](Constants.cs)

There is one trick that is important to point out. With `LoginPath` from [CK.DB.AspNet.OIddict.Configuration](Configuration.cs) set, the actual login will be called with `Redirect` instead of the usual `Challenge`. Hence the redirection is crafted directly.

If you run a flow that require the user consent, you have to provide a `ConsentPath`.
