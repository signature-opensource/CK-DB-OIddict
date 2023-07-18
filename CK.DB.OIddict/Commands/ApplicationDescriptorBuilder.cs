using System;
using System.Globalization;
using System.Text.Json;
using OpenIddict.Abstractions;

namespace CK.DB.OIddict.Commands
{
    /// <summary>
    /// Ease the creation of an <see cref="OpenIddictApplicationDescriptor"/>, with the use of <see cref="OpenIddictConstants"/>.
    /// </summary>
    public class ApplicationDescriptorBuilder
    {
        private readonly OpenIddictApplicationDescriptor _app;
        private bool _built;

        /// <summary>
        /// Consider calling <see cref="ApplicationDescriptorBuilder(string, string)"/>
        /// since those arguments are mandatory by default.
        /// Else-way you must disable the check on build by passing false to <see cref="Build"/>
        /// </summary>
        public ApplicationDescriptorBuilder() => _app = new OpenIddictApplicationDescriptor();

        public ApplicationDescriptorBuilder( string clientId, string clientSecret )
        {
            _app = new OpenIddictApplicationDescriptor
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
            };
        }

        /// <summary>
        /// Set <see cref="OpenIddictApplicationDescriptor.ConsentType"/>.
        /// </summary>
        /// <param name="consentType"></param>
        /// <returns></returns>
        public ApplicationDescriptorBuilder WithConsentType( string consentType )
        {
            _app.ConsentType = consentType;

            return this;
        }

        /// <summary>
        /// Set <see cref="OpenIddictApplicationDescriptor.DisplayName"/>.
        /// </summary>
        /// <param name="displayName"></param>
        /// <returns></returns>
        public ApplicationDescriptorBuilder WithDisplayName( string displayName )
        {
            _app.DisplayName = displayName;

            return this;
        }

        /// <summary>
        /// Add element to <see cref="OpenIddictApplicationDescriptor.DisplayNames"/>.
        /// If not set, will set the <see cref="OpenIddictApplicationDescriptor.DisplayName"/> property also.
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        public ApplicationDescriptorBuilder AddDisplayName( CultureInfo cultureInfo, string displayName )
        {
            _app.DisplayName ??= displayName;

            _app.DisplayNames[cultureInfo] = displayName;

            return this;
        }

        /// <summary>
        ///  Add element to <see cref="OpenIddictApplicationDescriptor.Permissions"/>.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public ApplicationDescriptorBuilder AddPermission( string permission )
        {
            _app.Permissions.Add( permission );

            return this;
        }

        /// <summary>
        /// Add Email and Profile scopes to <see cref="OpenIddictApplicationDescriptor.Permissions"/>.
        /// </summary>
        /// <returns></returns>
        public ApplicationDescriptorBuilder AddCommonScopes()
        {
            _app.Permissions.Add( OpenIddictConstants.Permissions.Scopes.Email );
            _app.Permissions.Add( OpenIddictConstants.Permissions.Scopes.Profile );

            return this;
        }

        /// <summary>
        /// Add scope to <see cref="OpenIddictApplicationDescriptor.Permissions"/>.
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public ApplicationDescriptorBuilder AddScope( string scope )
        {
            _app.Permissions.Add( $"{OpenIddictConstants.Permissions.Prefixes.Scope}{scope}" );

            return this;
        }

        /// <summary>
        /// Add Uri to <see cref="OpenIddictApplicationDescriptor.PostLogoutRedirectUris"/>.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public ApplicationDescriptorBuilder AddPostLogoutRedirectUri( Uri uri )
        {
            _app.PostLogoutRedirectUris.Add( uri );

            return this;
        }

        /// <summary>
        /// Add element to <see cref="OpenIddictApplicationDescriptor.Properties"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ApplicationDescriptorBuilder AddProperty( string name, JsonElement value )
        {
            _app.Properties[name] = value;

            return this;
        }

        /// <summary>
        /// Add Uri to <see cref="OpenIddictApplicationDescriptor.RedirectUris"/>.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public ApplicationDescriptorBuilder AddRedirectUri( Uri uri )
        {
            _app.RedirectUris.Add( uri );

            return this;
        }

        /// <summary>
        /// Add requirement to <see cref="OpenIddictApplicationDescriptor.Requirements"/>.
        /// </summary>
        /// <param name="requirement"></param>
        /// <returns></returns>
        public ApplicationDescriptorBuilder AddRequirement( string requirement )
        {
            _app.Requirements.Add( requirement );

            return this;
        }

        /// <summary>
        /// Set <see cref="OpenIddictApplicationDescriptor.Type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ApplicationDescriptorBuilder WithType( string type )
        {
            _app.Type = type;

            return this;
        }

        /// <summary>
        /// Ideal for development environment to quickly create an application ready for authorization code flow.
        /// Won't override any value nor update any non empty collection.
        /// </summary>
        /// <returns></returns>
        public ApplicationDescriptorBuilder EnsureCodeDefaults()
        {
            _app.ClientId ??= $"client-{Guid.NewGuid()}";
            _app.ClientSecret ??= $"secret-{Guid.NewGuid()}";

            _app.ConsentType ??= OpenIddictConstants.ConsentTypes.Explicit;

            _app.DisplayName ??= $"Default application {Guid.NewGuid()}";

            if( _app.Permissions.Count == 0 )
            {
                _app.Permissions.Add( OpenIddictConstants.Permissions.Endpoints.Authorization );
                _app.Permissions.Add( OpenIddictConstants.Permissions.Endpoints.Logout );
                _app.Permissions.Add( OpenIddictConstants.Permissions.Endpoints.Token );
                _app.Permissions.Add( OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode );
                _app.Permissions.Add( OpenIddictConstants.Permissions.ResponseTypes.Code );
                _app.Permissions.Add( OpenIddictConstants.Permissions.Scopes.Email );
                _app.Permissions.Add( OpenIddictConstants.Permissions.Scopes.Profile );
            }

            if( _app.Requirements.Count == 0 )
            {
                _app.Requirements.Add( OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange );
            }

            _app.Type ??= OpenIddictConstants.ClientTypes.Confidential;

            return this;
        }

        /// <summary>
        /// Build a ready to use <see cref="OpenIddictApplicationDescriptor"/>.
        /// </summary>
        /// <param name="throwOnMissingMandatory">By default, <see cref="OpenIddictApplicationDescriptor.ClientId"/>, <see cref="OpenIddictApplicationDescriptor.ClientSecret"/> have to be set. Pass false to ignore and not throw.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public OpenIddictApplicationDescriptor Build( bool throwOnMissingMandatory = true )
        {
            if( _built ) throw new InvalidOperationException( "Build() method can only be called once." );

            if( throwOnMissingMandatory )
            {
                if( _app.ClientId is null )
                    throw new InvalidOperationException( $"Required {_app.ClientId} is not set" );
                if( _app.ClientSecret is null )
                    throw new InvalidOperationException( $"Required {_app.ClientSecret} is not set" );
            }

            _built = true;

            return _app;
        }
    }
}
