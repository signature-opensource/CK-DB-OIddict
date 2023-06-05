using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CK.AspNet.Auth;
using CK.DB.Actor;
using CK.DB.AspNet.OpenIddictSql.Helpers;
using CK.SqlServer;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.AspNetCore.OpenIddictServerAspNetCoreConstants.Properties;


namespace CK.DB.AspNet.OpenIddictSql.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;
        private readonly IOpenIddictScopeManager _scopeManager;

        private readonly UserTable _userTable;

        private readonly string _challengeScheme;
        private readonly string _loginUrl;

        public AuthorizationController
        (
            IOpenIddictApplicationManager applicationManager,
            IOpenIddictAuthorizationManager authorizationManager,
            IOpenIddictScopeManager scopeManager,
            UserTable userTable,
            Configuration configuration
        )
        {
            _applicationManager = applicationManager;
            _authorizationManager = authorizationManager;
            _scopeManager = scopeManager;
            _userTable = userTable;

            _challengeScheme = configuration.AuthenticationScheme;
            _loginUrl = configuration.LoginPath;
        }

        // This could be a strategy pattern to avoid the if statement.
        /// <summary>
        /// Challenge adapter that handles standard Challenge and WebFrontAuth.
        /// </summary>
        /// <param name="redirectUri">The uri that is passed as
        /// <see cref="Microsoft.AspNetCore.Authentication.AuthenticationProperties.RedirectUri"/>
        /// to <see cref="AuthenticationProperties"/>.</param>
        /// <returns>Challenge <see cref="_challengeScheme"/></returns>
        private IActionResult HandleChallenge( string redirectUri )
        {
            if( _challengeScheme == WebFrontAuthOptions.OnlyAuthenticationScheme )
                return Redirect
                (
                    $"{_loginUrl}{QueryString.Create( "ReturnUrl", redirectUri )}"
                );

            return Challenge
            (
                authenticationSchemes: _challengeScheme,
                properties: new AuthenticationProperties { RedirectUri = redirectUri }
            );
        }

        [HttpGet( ConstantsConfiguration.AuthorizeUri )]
        [HttpPost( ConstantsConfiguration.AuthorizeUri )]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AuthorizeAsync()
        {
            var oidcRequest = HttpContext.GetOpenIddictServerRequest()
                           ?? throw new InvalidOperationException( "The OpenID Connect request cannot be retrieved." );

            // Try to retrieve the user principal stored in the authentication cookie and redirect
            // the user agent to the login page (or to an external provider) in the following cases:
            //
            //  - If the user principal can't be extracted or the cookie is too old.
            //  - If prompt=login was specified by the client application.
            //  - If a max_age parameter was provided and the authentication cookie is not considered "fresh" enough.

            var authResult = await HttpContext.AuthenticateAsync( _challengeScheme );

            var isAuthenticated = authResult.Principal?.Identity != null
                               && authResult.Principal           != null
                               && authResult.Principal.Identity.IsAuthenticated;
            var isAuthCookieStale =
            oidcRequest.MaxAge                                      != null
         && authResult.Properties?.IssuedUtc                        != null
         && DateTimeOffset.UtcNow - authResult.Properties.IssuedUtc > TimeSpan.FromSeconds( oidcRequest.MaxAge.Value );

            if
            (
                authResult is not { Succeeded: true }
             || !isAuthenticated
             || oidcRequest.HasPrompt( Prompts.Login )
             || isAuthCookieStale
            )
            {
                // If the client application requested promptless authentication,
                // return an error indicating that the user is not logged in.
                if( oidcRequest.HasPrompt( Prompts.None ) )
                {
                    return Forbid
                    (
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties
                        (
                            new Dictionary<string, string?>
                            {
                                [Error] = Errors.LoginRequired,
                                [ErrorDescription] = "The user is not logged in.",
                            }
                        )
                    );
                }

                // To avoid endless login -> authorization redirects, the prompt=login flag
                // is removed from the authorization request payload before redirecting the user.
                var prompt = string.Join( " ", oidcRequest.GetPrompts().Remove( Prompts.Login ) );

                var parameters = Request.HasFormContentType
                ? Request.Form.Where( parameter => parameter.Key  != Parameters.Prompt ).ToList()
                : Request.Query.Where( parameter => parameter.Key != Parameters.Prompt ).ToList();

                parameters.Add( KeyValuePair.Create( Parameters.Prompt, new StringValues( prompt ) ) );

                var redirectUri = Request.PathBase + Request.Path + QueryString.Create( parameters );

                return HandleChallenge( redirectUri );
            }

            // Retrieve the profile of the logged in user.
            var userName = authResult.Principal.FindFirstValue( Claims.Name );
            var userId = 0;
            using( var sqlCallContext = new SqlStandardCallContext() )
            {
                userId = await _userTable.FindByNameAsync( sqlCallContext, userName );
            }

            if( userId <= 0 ) throw new InvalidOperationException( "The user details cannot be retrieved." );

            // Retrieve the application details from the database.
            var application = await _applicationManager.FindByClientIdAsync( oidcRequest.ClientId! )
                           ?? throw new InvalidOperationException
                              (
                                  "Details concerning the calling client application cannot be found."
                              );

            // Retrieve the permanent authorizations associated with the user and the calling client application.
            var authorizations = await _authorizationManager.FindAsync
            (
                subject: userName,
                client: await _applicationManager.GetIdAsync( application ),
                status: Statuses.Valid,
                type: AuthorizationTypes.Permanent,
                scopes: oidcRequest.GetScopes()
            ).ToListAsync();

            switch( await _applicationManager.GetConsentTypeAsync( application ) )
            {
                // If the consent is external (e.g when authorizations are granted by a sysadmin),
                // immediately return an error if no authorization can be found in the database.
                case ConsentTypes.External when !authorizations.Any():
                    return Forbid
                    (
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties
                        (
                            new Dictionary<string, string?>
                            {
                                [Error] = Errors.ConsentRequired,
                                [ErrorDescription] =
                                "The logged in user is not allowed to access this client application.",
                            }
                        )
                    );

                // If the consent is implicit or if an authorization was found,
                // return an authorization response without displaying the consent form.
                case ConsentTypes.Implicit:
                case ConsentTypes.External when authorizations.Any():
                case ConsentTypes.Explicit when authorizations.Any() && !oidcRequest.HasPrompt( Prompts.Consent ):
                    // Create the claims-based identity that will be used by OpenIddict to generate tokens.
                    var identity = new ClaimsIdentity
                    (
                        authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                        nameType: Claims.Name,
                        roleType: Claims.Role
                    );

                    // Add the claims that will be persisted in the tokens.
                    identity.SetClaim( Claims.Subject, userName )
                            .SetClaim( Claims.Email, userName )
                            .SetClaim( Claims.Name, userName );
                    // no role

                    // Note: in this sample, the granted scopes match the requested scope
                    // but you may want to allow the user to uncheck specific scopes.
                    // For that, simply restrict the list of scopes before calling SetScopes.
                    identity.SetScopes( oidcRequest.GetScopes() );
                    identity.SetResources
                    (
                        await _scopeManager.ListResourcesAsync( identity.GetScopes() ).ToListAsync()
                    );

                    // Automatically create a permanent authorization to avoid requiring explicit consent
                    // for future authorization or token requests containing the same scopes.
                    var authorization = authorizations.LastOrDefault();
                    authorization ??= await _authorizationManager.CreateAsync
                    (
                        identity: identity,
                        subject: userName,
                        client: await _applicationManager.GetIdAsync( application ),
                        type: AuthorizationTypes.Permanent,
                        scopes: identity.GetScopes()
                    );

                    identity.SetAuthorizationId( await _authorizationManager.GetIdAsync( authorization ) );
                    identity.SetDestinations( GetDestinations );

                    return SignIn
                    (
                        new ClaimsPrincipal( identity ),
                        OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                    );

                // At this point, no authorization was found in the database and an error must be returned
                // if the client application specified prompt=none in the authorization request.
                case ConsentTypes.Explicit when oidcRequest.HasPrompt( Prompts.None ):
                case ConsentTypes.Systematic when oidcRequest.HasPrompt( Prompts.None ):
                    return Forbid
                    (
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties
                        (
                            new Dictionary<string, string?>
                            {
                                [Error] = Errors.ConsentRequired,
                                [ErrorDescription] = "Interactive user consent is required.",
                            }
                        )
                    );

                // In every other case, render the consent form.
                // default: return View(new AuthorizeViewModel
                // {
                //     ApplicationName = await _applicationManager.GetLocalizedDisplayNameAsync(application),
                //     Scope = request.Scope
                // });
                default: return await SimulateUserAcceptConsentAsync();
                // default:
                // {
                // var prompt = string.Join( " ", request.GetPrompts().Remove( Prompts.Login ) );
                //
                // var parameters = Request.HasFormContentType
                // ? Request.Form.Where( parameter => parameter.Key  != Parameters.Prompt ).ToList()
                // : Request.Query.Where( parameter => parameter.Key != Parameters.Prompt ).ToList();
                //
                // parameters.Add( KeyValuePair.Create( Parameters.Prompt, new StringValues( prompt ) ) );
                //
                // var redirectUri = Request.PathBase + Request.Path + QueryString.Create( parameters );
                //
                // var returnUrl = HttpUtility.UrlEncode( redirectUri );
                // var consentRedirectUrl = $"/Authorization/Consent?returnUrl={returnUrl}";
                // return Redirect( consentRedirectUrl );
                // }
            }
        }

        public async Task<IActionResult> SimulateUserAcceptConsentAsync()
        {
            var result = await AcceptAsync();

            return result;
        }

        [Authorize, FormValueRequired( "submit.Accept" )]
        [HttpPost( ConstantsConfiguration.AuthorizeUri ), ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptAsync()
        {
            var request = HttpContext.GetOpenIddictServerRequest()
                       ?? throw new InvalidOperationException( "The OpenID Connect request cannot be retrieved." );

            // Retrieve the profile of the logged in user.
            var userName = User.FindFirstValue( Claims.Name );
            var userId = 0;
            using( var sqlCallContext = new SqlStandardCallContext() )
            {
                userId = await _userTable.FindByNameAsync( sqlCallContext, userName );
            }

            if( userId <= 0 ) throw new InvalidOperationException( "The user details cannot be retrieved." );

            // Retrieve the application details from the database.
            var application = await _applicationManager.FindByClientIdAsync( request.ClientId )
                           ?? throw new InvalidOperationException
                              (
                                  "Details concerning the calling client application cannot be found."
                              );

            // Retrieve the permanent authorizations associated with the user and the calling client application.
            var authorizations = await _authorizationManager.FindAsync
            (
                subject: userName,
                client: await _applicationManager.GetIdAsync( application ),
                status: Statuses.Valid,
                type: AuthorizationTypes.Permanent,
                scopes: request.GetScopes()
            ).ToListAsync();

            // Note: the same check is already made in the other action but is repeated
            // here to ensure a malicious user can't abuse this POST-only endpoint and
            // force it to return a valid response without the external authorization.
            if
            (
                !authorizations.Any()
             && await _applicationManager.HasConsentTypeAsync( application, ConsentTypes.External )
            )
            {
                return Forbid
                (
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties
                    (
                        new Dictionary<string, string?>
                        {
                            [Error] = Errors.ConsentRequired,
                            [ErrorDescription] = "The logged in user is not allowed to access this client application.",
                        }
                    )
                );
            }

            // Create the claims-based identity that will be used by OpenIddict to generate tokens.
            var identity = new ClaimsIdentity
            (
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role
            );

            // Add the claims that will be persisted in the tokens.
            identity.SetClaim( Claims.Subject, userName )
                    .SetClaim( Claims.Email, userName )
                    .SetClaim( Claims.Name, userName );

            // Note: in this sample, the granted scopes match the requested scope
            // but you may want to allow the user to uncheck specific scopes.
            // For that, simply restrict the list of scopes before calling SetScopes.
            identity.SetScopes( request.GetScopes() );
            // identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

            // Automatically create a permanent authorization to avoid requiring explicit consent
            // for future authorization or token requests containing the same scopes.
            var authorization = authorizations.LastOrDefault();
            authorization ??= await _authorizationManager.CreateAsync
            (
                identity: identity,
                subject: userName,
                client: await _applicationManager.GetIdAsync( application ),
                type: AuthorizationTypes.Permanent,
                scopes: identity.GetScopes()
            );

            identity.SetAuthorizationId( await _authorizationManager.GetIdAsync( authorization ) );
            identity.SetDestinations( GetDestinations );

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn( new ClaimsPrincipal( identity ), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme );
        }

        [Authorize, FormValueRequired( "submit.Deny" )]
        [HttpPost( ConstantsConfiguration.AuthorizeUri ), ValidateAntiForgeryToken]
        // Notify OpenIddict that the authorization grant has been denied by the resource owner
        // to redirect the user agent to the client application using the appropriate response_mode.
        public IActionResult Deny() => Forbid( OpenIddictServerAspNetCoreDefaults.AuthenticationScheme );


        [HttpPost( ConstantsConfiguration.TokenUri ), IgnoreAntiforgeryToken, Produces( "application/json" )]
        public async Task<IActionResult> ExchangeAsync()
        {
            var request = HttpContext.GetOpenIddictServerRequest()
                       ?? throw new InvalidOperationException( "The OpenID Connect request cannot be retrieved." );

            if
            (
                !request.IsAuthorizationCodeGrantType()
             && !request.IsRefreshTokenGrantType()
            )
                throw new InvalidOperationException( "The specified grant type is not supported." );


            // Retrieve the claims principal stored in the authorization code/refresh token.
            var result = await HttpContext.AuthenticateAsync( OpenIddictServerAspNetCoreDefaults.AuthenticationScheme );

            // Retrieve the user profile corresponding to the authorization code/refresh token.
            var userName = result.Principal?.GetClaim( Claims.Subject );

            //todo: get user from db
            if( userName is null )
            {
                return Forbid
                (
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties
                    (
                        new Dictionary<string, string>
                        {
                            [Error] = Errors.InvalidGrant,
                            [ErrorDescription] =
                            "The token is no longer valid."
                        }
                    )
                );
            }

            var identity = new ClaimsIdentity
            (
                result.Principal!.Claims,
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role
            );

            // Override the user claims present in the principal in case they
            // changed since the authorization code/refresh token was issued.
            identity.SetClaim( Claims.Subject, userName )
                    .SetClaim( Claims.Email, userName )
                    .SetClaim( Claims.Name, userName );
            // .SetClaims(Claims.Role, new List<string>().ToImmutableArray());

            identity.SetDestinations( GetDestinations );

            var claimsPrincipal = new ClaimsPrincipal( identity );

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn( claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme );
        }

        private static IEnumerable<string> GetDestinations( Claim claim )
        {
            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            switch( claim.Type )
            {
                case Claims.Name:
                    yield return Destinations.AccessToken;

                    if( claim.Subject.HasScope( Scopes.Profile ) )
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Email:
                    yield return Destinations.AccessToken;

                    if( claim.Subject.HasScope( Scopes.Email ) )
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;

                    if( claim.Subject.HasScope( Scopes.Roles ) )
                        yield return Destinations.IdentityToken;

                    yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }
    }
}
