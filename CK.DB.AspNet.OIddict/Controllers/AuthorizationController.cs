using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CK.DB.AspNet.OIddict.Helpers;
using CK.DB.AspNet.OIddict.Identity;
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


namespace CK.DB.AspNet.OIddict.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;
        private readonly IOpenIddictScopeManager _scopeManager;

        private readonly IIdentityStrategy _identityStrategy;

        private readonly string _challengeScheme;
        private readonly string? _loginUrl;
        private readonly string? _consentUrl;

        public AuthorizationController
        (
            IOpenIddictApplicationManager applicationManager,
            IOpenIddictAuthorizationManager authorizationManager,
            IOpenIddictScopeManager scopeManager,
            Configuration configuration,
            IIdentityStrategy identityStrategy
        )
        {
            _applicationManager = applicationManager;
            _authorizationManager = authorizationManager;
            _scopeManager = scopeManager;
            _identityStrategy = identityStrategy;

            _challengeScheme = configuration.AuthenticationScheme;
            _loginUrl = configuration.LoginPath;
            _consentUrl = configuration.ConsentPath;
        }

        // This could be a strategy pattern to avoid the if statement.
        /// <summary>
        /// Challenge adapter that handles standard Challenge and custom redirect.
        /// </summary>
        /// <param name="redirectUri">The uri that is passed as
        /// <see cref="Microsoft.AspNetCore.Authentication.AuthenticationProperties.RedirectUri"/>
        /// to <see cref="AuthenticationProperties"/>.</param>
        /// <returns>Challenge <see cref="_challengeScheme"/></returns>
        private IActionResult HandleChallenge( string redirectUri )
        {
            if( _loginUrl is not null )
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

        [HttpGet( Constants.AuthorizeUri )]
        [HttpPost( Constants.AuthorizeUri )]
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

            #region Challenge

            var isAuthenticated = authResult.Principal?.Identity != null
                               && authResult.Principal           != null
                               && authResult.Principal.Identity.IsAuthenticated;

            var isAuthCookieStale =
            oidcRequest.MaxAge                                      != null
         && authResult.Properties?.IssuedUtc                        != null
         && DateTimeOffset.UtcNow - authResult.Properties.IssuedUtc > TimeSpan.FromSeconds( oidcRequest.MaxAge.Value );

            var shouldChallenge = authResult is not { Succeeded: true }
                               || !isAuthenticated
                               || oidcRequest.HasPrompt( Prompts.Login )
                               || isAuthCookieStale;
            if( shouldChallenge )
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

            #endregion

            var authenticationInfo = await _identityStrategy.ValidateAuthAsync( authResult.Principal );
            if( authenticationInfo is null )
                throw new InvalidOperationException( "The user details cannot be retrieved." );
            var userName = authenticationInfo.UserName;

            var application = await _applicationManager.FindByClientIdAsync( oidcRequest.ClientId! )
                           ?? throw new InvalidOperationException
                              (
                                  "Details concerning the calling client application cannot be found."
                              );

            var scopes = oidcRequest.GetScopes();
            // Retrieve the permanent authorizations associated with the user and the calling client application.
            var authorizations = await _authorizationManager.FindAsync
            (
                subject: userName,
                client: await _applicationManager.GetIdAsync( application ),
                status: Statuses.Valid,
                type: AuthorizationTypes.Permanent,
                scopes: scopes
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
                    _identityStrategy.SetUserClaims( identity, authenticationInfo, scopes );

                    // Note: in this sample, the granted scopes match the requested scope
                    // but you may want to allow the user to uncheck specific scopes.
                    // For that, simply restrict the list of scopes before calling SetScopes.
                    identity.SetScopes( scopes );
                    // Uncomment next line when Scope Store is implemented
                    // identity.SetResources( await _scopeManager.ListResourcesAsync( identity.GetScopes() ).ToListAsync() );

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

                default: // Redirect to the consent form
                {
                    var prompt = string.Join( " ", oidcRequest.GetPrompts().Remove( Prompts.Login ) );

                    var parameters = Request.HasFormContentType
                    ? Request.Form.Where( parameter => parameter.Key  != Parameters.Prompt ).ToList()
                    : Request.Query.Where( parameter => parameter.Key != Parameters.Prompt ).ToList();

                    parameters.Add( KeyValuePair.Create( Parameters.Prompt, new StringValues( prompt ) ) );

                    var applicationName = await _applicationManager.GetDisplayNameAsync( application );
                    parameters.Add( KeyValuePair.Create( "applicationName", new StringValues( applicationName ) ) );

                    var queryString = QueryString.Create( parameters );
                    var consentUrl = _consentUrl + queryString;

                    return Redirect( consentUrl );
                }
            }
        }

        public async Task<IActionResult> SimulateUserAcceptConsentAsync()
        {
            var result = await AcceptAsync();

            return result;
        }

        [Authorize, FormValueRequired( "submit.Accept" )]
        [HttpPost( Constants.AuthorizeUri ), ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptAsync()
        {
            var oidcRequest = HttpContext.GetOpenIddictServerRequest()
                           ?? throw new InvalidOperationException( "The OpenID Connect request cannot be retrieved." );

            var authenticationInfo = await _identityStrategy.ValidateAuthAsync( User );
            if( authenticationInfo is null )
                throw new InvalidOperationException( "The user details cannot be retrieved." );
            var userName = authenticationInfo.UserName;

            var application = await _applicationManager.FindByClientIdAsync( oidcRequest.ClientId )
                           ?? throw new InvalidOperationException
                              (
                                  "Details concerning the calling client application cannot be found."
                              );

            var scopes = oidcRequest.GetScopes();
            // Retrieve the permanent authorizations associated with the user and the calling client application.
            var authorizations = await _authorizationManager.FindAsync
            (
                subject: userName,
                client: await _applicationManager.GetIdAsync( application ),
                status: Statuses.Valid,
                type: AuthorizationTypes.Permanent,
                scopes: scopes
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
            _identityStrategy.SetUserClaims( identity, authenticationInfo, scopes );

            // Note: in this sample, the granted scopes match the requested scope
            // but you may want to allow the user to uncheck specific scopes.
            // For that, simply restrict the list of scopes before calling SetScopes.
            identity.SetScopes( scopes );
            // Uncomment next line when Scope Store is implemented
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
        [HttpPost( Constants.AuthorizeUri ), ValidateAntiForgeryToken]
        // Notify OpenIddict that the authorization grant has been denied by the resource owner
        // to redirect the user agent to the client application using the appropriate response_mode.
        public IActionResult Deny() => Forbid( OpenIddictServerAspNetCoreDefaults.AuthenticationScheme );


        [HttpPost( Constants.TokenUri ), IgnoreAntiforgeryToken, Produces( "application/json" )]
        public async Task<IActionResult> ExchangeAsync()
        {
            var oidcRequest = HttpContext.GetOpenIddictServerRequest()
                           ?? throw new InvalidOperationException( "The OpenID Connect request cannot be retrieved." );

            if
            (
                !oidcRequest.IsAuthorizationCodeGrantType()
             && !oidcRequest.IsRefreshTokenGrantType()
            )
                throw new InvalidOperationException( "The specified grant type is not supported." );


            // Retrieve the claims principal stored in the authorization code/refresh token.
            var authResult = await HttpContext.AuthenticateAsync
            (
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            );

            // Retrieve the user profile corresponding to the authorization code/refresh token.
            var authenticationInfo = await _identityStrategy.ValidateAuthAsync( authResult.Principal );

            if( authenticationInfo is null )
            {
                return Forbid
                (
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties
                    (
                        new Dictionary<string, string?>
                        {
                            [Error] = Errors.InvalidGrant,
                            [ErrorDescription] = "The token is no longer valid.",
                        }
                    )
                );
            }

            var scopes = authResult.Principal!.Claims
                                   .Where( c => c.Type == Claims.Private.Scope )
                                   .Select( c => c.Value )
                                   .ToImmutableArray();

            var identity = new ClaimsIdentity
            (
                authResult.Principal!.Claims,
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role
            );

            // Override the user claims present in the principal in case they
            // changed since the authorization code/refresh token was issued.
            // SetUserClaims( identity, authenticationInfo, scopes, _setUserClaims );
            _identityStrategy.SetUserClaims( identity, authenticationInfo, scopes );

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

            Debug.Assert( claim.Subject != null, "claim.Subject != null" );

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
