// using System;
// using System.Collections.Generic;
// using System.Globalization;
// using System.Text.Json;
// using CK.DB.OIddict.Entities;
// using OpenIddict.Abstractions;
//
// namespace CK.DB.OIddict.Commands
// {
//     /// <summary>
//     /// Ease the creation of an <see cref="Application"/>, with the use of <see cref="OpenIddictConstants"/>.
//     /// </summary>
//     public class ApplicationBuilder
//     {
//         private readonly Application _app;
//         private bool _built;
//
//         /// <summary>
//         /// Consider calling <see cref="ApplicationBuilder(string, string, Guid?)"/>
//         /// since those arguments are mandatory by default.
//         /// Else-way you must disable the check on build by passing false to <see cref="Build"/>
//         /// </summary>
//         public ApplicationBuilder( Guid? applicationId = null )
//         {
//             _app = applicationId is null
//             ? new Application()
//             : new Application() { ApplicationId = applicationId.Value };
//         }
//
//         public ApplicationBuilder( string clientId, string clientSecret, Guid? applicationId = null )
//         : this( applicationId )
//         {
//             _app.ClientId = clientId;
//             _app.ClientSecret = clientSecret;
//         }
//
//         /// <summary>
//         /// Set <see cref="Application.ConsentType"/>.
//         /// </summary>
//         /// <param name="consentType"></param>
//         /// <returns></returns>
//         public ApplicationBuilder WithConsentType( string consentType )
//         {
//             _app.ConsentType = consentType;
//
//             return this;
//         }
//
//         /// <summary>
//         /// Set <see cref="Application.DisplayName"/>.
//         /// </summary>
//         /// <param name="displayName"></param>
//         /// <returns></returns>
//         public ApplicationBuilder WithDisplayName( string displayName )
//         {
//             _app.DisplayName = displayName;
//
//             return this;
//         }
//
//         /// <summary>
//         /// Add element to <see cref="Application.DisplayNames"/>.
//         /// If not set, will set the <see cref="Application.DisplayName"/> property also.
//         /// </summary>
//         /// <param name="cultureInfo"></param>
//         /// <param name="displayName"></param>
//         /// <returns></returns>
//         public ApplicationBuilder AddDisplayName( CultureInfo cultureInfo, string displayName )
//         {
//             _app.DisplayName ??= displayName;
//             _app.DisplayNames ??= new Dictionary<CultureInfo, string>();
//
//             _app.DisplayNames[cultureInfo] = displayName;
//
//             return this;
//         }
//
//         /// <summary>
//         ///  Add element to <see cref="Application.Permissions"/>.
//         /// </summary>
//         /// <param name="permission"></param>
//         /// <returns></returns>
//         public ApplicationBuilder AddPermission( string permission )
//         {
//             _app.Permissions ??= new HashSet<string>();
//
//             _app.Permissions.Add( permission );
//
//             return this;
//         }
//
//         /// <summary>
//         /// Add Email and Profile scopes to <see cref="Application.Permissions"/>.
//         /// </summary>
//         /// <returns></returns>
//         public ApplicationBuilder AddCommonScopes()
//         {
//             _app.Permissions ??= new HashSet<string>();
//
//             _app.Permissions.Add( OpenIddictConstants.Permissions.Scopes.Email );
//             _app.Permissions.Add( OpenIddictConstants.Permissions.Scopes.Profile );
//
//             return this;
//         }
//
//         /// <summary>
//         /// Add scope to <see cref="Application.Permissions"/>.
//         /// </summary>
//         /// <param name="scope"></param>
//         /// <returns></returns>
//         public ApplicationBuilder AddScope( string scope )
//         {
//             _app.Permissions ??= new HashSet<string>();
//
//             _app.Permissions.Add( $"{OpenIddictConstants.Permissions.Prefixes.Scope}{scope}" );
//
//             return this;
//         }
//
//         /// <summary>
//         /// Add Uri to <see cref="Application.PostLogoutRedirectUris"/>.
//         /// </summary>
//         /// <param name="uri"></param>
//         /// <returns></returns>
//         public ApplicationBuilder AddPostLogoutRedirectUri( Uri uri )
//         {
//             _app.PostLogoutRedirectUris ??= new HashSet<Uri>();
//
//             _app.PostLogoutRedirectUris.Add( uri );
//
//             return this;
//         }
//
//         /// <summary>
//         /// Add element to <see cref="Application.Properties"/>.
//         /// </summary>
//         /// <param name="name"></param>
//         /// <param name="value"></param>
//         /// <returns></returns>
//         public ApplicationBuilder AddProperty( string name, JsonElement value )
//         {
//             _app.Properties ??= new Dictionary<string, JsonElement>();
//
//             _app.Properties[name] = value;
//
//             return this;
//         }
//
//         /// <summary>
//         /// Add Uri to <see cref="Application.RedirectUris"/>.
//         /// </summary>
//         /// <param name="uri"></param>
//         /// <returns></returns>
//         public ApplicationBuilder AddRedirectUri( Uri uri )
//         {
//             _app.RedirectUris ??= new HashSet<Uri>();
//
//             _app.RedirectUris.Add( uri );
//
//             return this;
//         }
//
//         /// <summary>
//         /// Add requirement to <see cref="Application.Requirements"/>.
//         /// </summary>
//         /// <param name="requirement"></param>
//         /// <returns></returns>
//         public ApplicationBuilder AddRequirement( string requirement )
//         {
//             _app.Requirements ??= new HashSet<string>();
//
//             _app.Requirements.Add( requirement );
//
//             return this;
//         }
//
//         /// <summary>
//         /// Set <see cref="Application.Type"/>.
//         /// </summary>
//         /// <param name="type"></param>
//         /// <returns></returns>
//         public ApplicationBuilder WithType( string type )
//         {
//             _app.Type = type;
//
//             return this;
//         }
//
//         /// <summary>
//         /// Ideal for development environment to quickly create an application ready for authorization code flow.
//         /// Won't override any value nor update any non empty collection.
//         /// </summary>
//         /// <returns></returns>
//         public ApplicationBuilder EnsureCodeDefaults()
//         {
//             _app.ClientId ??= $"client-{Guid.NewGuid()}";
//             _app.ClientSecret ??= $"secret-{Guid.NewGuid()}";
//
//             _app.ConsentType ??= OpenIddictConstants.ConsentTypes.Explicit;
//
//             _app.DisplayName ??= $"Default application {Guid.NewGuid()}";
//
//             _app.Permissions ??= new HashSet<string>();
//             if( _app.Permissions.Count == 0 )
//             {
//                 _app.Permissions.Add( OpenIddictConstants.Permissions.Endpoints.Authorization );
//                 _app.Permissions.Add( OpenIddictConstants.Permissions.Endpoints.Logout );
//                 _app.Permissions.Add( OpenIddictConstants.Permissions.Endpoints.Token );
//                 _app.Permissions.Add( OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode );
//                 _app.Permissions.Add( OpenIddictConstants.Permissions.ResponseTypes.Code );
//                 _app.Permissions.Add( OpenIddictConstants.Permissions.Scopes.Email );
//                 _app.Permissions.Add( OpenIddictConstants.Permissions.Scopes.Profile );
//             }
//
//             _app.Requirements ??= new HashSet<string>();
//             if( _app.Requirements.Count == 0 )
//             {
//                 _app.Requirements.Add( OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange );
//             }
//
//             _app.Type ??= OpenIddictConstants.ClientTypes.Confidential;
//
//             return this;
//         }
//
//         /// <summary>
//         /// Build a ready to use <see cref="Application"/>.
//         /// </summary>
//         /// <param name="throwOnMissingMandatory">By default, <see cref="Application.ClientId"/>, <see cref="Application.ClientSecret"/> have to be set. Pass false to ignore and not throw.</param>
//         /// <returns></returns>
//         /// <exception cref="InvalidOperationException"></exception>
//         public Application Build( bool throwOnMissingMandatory = true )
//         {
//             if( _built ) throw new InvalidOperationException( "Build() method can only be called once." );
//
//             if( throwOnMissingMandatory )
//             {
//                 if( _app.ClientId is null )
//                     throw new InvalidOperationException( $"Required {_app.ClientId} is not set" );
//                 if( _app.ClientSecret is null )
//                     throw new InvalidOperationException( $"Required {_app.ClientSecret} is not set" );
//             }
//
//             _built = true;
//
//             return _app;
//         }
//     }
// }
