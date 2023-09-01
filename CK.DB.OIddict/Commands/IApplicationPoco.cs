using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using CK.Core;
using CK.DB.OIddict.Entities;
using OpenIddict.Abstractions;
using static CK.DB.OIddict.Dapper.JsonTypeConverter;

namespace CK.DB.OIddict.Commands
{
    public interface IApplicationPoco : IPoco
    {
        public Guid? ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the client identifier associated with the application.
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret associated with the application.
        /// Note: depending on the application manager used when creating it,
        /// this property may be hashed or encrypted for security reasons.
        /// </summary>
        public string? ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the consent type associated with the application.
        /// </summary>
        public string? ConsentType { get; set; }

        /// <summary>
        /// Gets or sets the display name associated with the application.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets the localized display names associated with the application.
        /// Key: CultureName, Value: DisplayName.
        /// </summary>
        public Dictionary<string, string>? DisplayNames { get; set; }

        /// <summary>Gets the permissions associated with the application.</summary>
        public HashSet<string>? Permissions { get; set; }

        /// <summary>
        /// Gets the post-logout redirect URIs associated with the application.
        /// </summary>
        public HashSet<string>? PostLogoutRedirectUris { get; set; }

        /// <summary>
        /// Gets the additional properties associated with the application.
        /// </summary>
        public IPropertiesPoco? Properties { get; set; }

        /// <summary>
        /// Gets the redirect URIs associated with the application.
        /// </summary>
        public HashSet<string>? RedirectUris { get; set; }

        /// <summary>
        /// Gets the requirements associated with the application.
        /// </summary>
        public HashSet<string>? Requirements { get; set; }

        /// <summary>
        /// Gets or sets the application type associated with the application.
        /// </summary>
        public string? Type { get; set; }
    }

    public interface IPropertiesPoco : IPoco
    {
        string? PropertiesJson { get; set; }
    }

    public class ApplicationPocoFactory : IAutoService
    {
        private readonly PocoDirectory _pocoDirectory;

        public ApplicationPocoFactory( PocoDirectory pocoDirectory ) => _pocoDirectory = pocoDirectory;

        public IApplicationPoco CreatePoco( Application application ) => _pocoDirectory.Create<IApplicationPoco>
        (
            a =>
            {
                a.ApplicationId = application.ApplicationId;
                a.ClientId = application.ClientId;
                a.ClientSecret = application.ClientSecret;
                a.ConsentType = application.ConsentType;
                a.DisplayName = application.DisplayName;
                a.DisplayNames = CreateDisplayNamesPoco( application.DisplayNames );
                a.Permissions = application.Permissions;
                a.PostLogoutRedirectUris = CreateUrisPoco( application.PostLogoutRedirectUris );
                a.Properties = CreatePropertiesPoco( application.Properties );
                a.RedirectUris = CreateUrisPoco( application.RedirectUris );
                a.Requirements = application.Requirements;
                a.Type = application.Type;
            }
        );

        public Application Create( IApplicationPoco poco )
        {//TODO: maybe it has to have an ApplicationId
            var app = poco.ApplicationId is not null
            ? new Application() { ApplicationId = poco.ApplicationId.Value }
            : new Application();

            app.ClientId = poco.ClientId;
            app.ClientSecret = poco.ClientSecret;
            app.ConsentType = poco.ConsentType;
            app.DisplayName = poco.DisplayName;
            app.DisplayNames = CreateDisplayNames( poco.DisplayNames );
            app.Permissions = poco.Permissions;
            app.PostLogoutRedirectUris = CreateUris( poco.PostLogoutRedirectUris );
            app.Properties = CreateProperties( poco.Properties );
            app.RedirectUris = CreateUris( poco.RedirectUris );
            app.Requirements = poco.Requirements;
            app.Type = poco.Type;

            return app;
        }

        public IApplicationPoco CreatePoco( OpenIddictApplicationDescriptor descriptor ) =>
        _pocoDirectory.Create<IApplicationPoco>
        (
            a =>
            {
                a.ClientId = descriptor.ClientId;
                a.ClientSecret = descriptor.ClientSecret;
                a.ConsentType = descriptor.ConsentType;
                a.DisplayName = descriptor.DisplayName;
                a.DisplayNames = CreateDisplayNamesPoco( descriptor.DisplayNames );
                a.Permissions = descriptor.Permissions;
                a.PostLogoutRedirectUris = CreateUrisPoco( descriptor.PostLogoutRedirectUris );
                a.Properties = CreatePropertiesPoco( descriptor.Properties );
                a.RedirectUris = CreateUrisPoco( descriptor.RedirectUris );
                a.Requirements = descriptor.Requirements;
                a.Type = descriptor.Type;
            }
        );

        public OpenIddictApplicationDescriptor CreateDescriptor( IApplicationPoco poco )
        {
            var app = new OpenIddictApplicationDescriptor
            {
                ClientId = poco.ClientId,
                ClientSecret = poco.ClientSecret,
                ConsentType = poco.ConsentType,
                DisplayName = poco.DisplayName,
                Type = poco.Type,
            };

            var displayNames = CreateDisplayNames( poco.DisplayNames );
            if( displayNames is not null ) app.DisplayNames.AddRange( displayNames );

            var permissions = poco.Permissions;
            if( permissions is not null ) app.Permissions.AddRange( permissions );

            var postLogoutRedirectUris = CreateUris( poco.PostLogoutRedirectUris );
            if( postLogoutRedirectUris is not null ) app.PostLogoutRedirectUris.AddRange( postLogoutRedirectUris );

            var properties = CreateProperties( poco.Properties );
            if( properties is not null ) app.Properties.AddRange( properties );

            var redirectUris = CreateUris( poco.RedirectUris );
            if( redirectUris is not null ) app.RedirectUris.AddRange( redirectUris );

            var requirements = poco.Requirements;
            if( requirements is not null ) app.Requirements.AddRange( requirements );

            return app;
        }

        internal string CreateCultureInfoPoco( CultureInfo cultureInfo ) => cultureInfo.Name;

        internal CultureInfo CreateCultureInfo( string poco ) => new( poco );

        internal Dictionary<string, string>? CreateDisplayNamesPoco
        (
            Dictionary<CultureInfo, string>? displayNames
        )
            => displayNames?.ToDictionary
            (
                d => CreateCultureInfoPoco( d.Key ),
                d => d.Value
            );

        internal Dictionary<CultureInfo, string>? CreateDisplayNames( Dictionary<string, string>? poco )
            => poco?.ToDictionary
            (
                p => CreateCultureInfo( p.Key ),
                p => p.Value
            );

        internal string CreateUriPoco( Uri uri ) => uri.ToString();

        internal Uri CreateUri( string poco ) => new( poco );

        internal HashSet<string>? CreateUrisPoco( HashSet<Uri>? uris ) => uris?.Select( CreateUriPoco ).ToHashSet();

        internal HashSet<Uri>? CreateUris( HashSet<string>? poco ) => poco?.Select( CreateUri ).ToHashSet();

        internal Dictionary<string, JsonElement>? CreateProperties( IPropertiesPoco? poco )
            => FromJson<Dictionary<string, JsonElement>>( poco?.PropertiesJson );

        internal IPropertiesPoco CreatePropertiesPoco( Dictionary<string, JsonElement>? properties )
            => _pocoDirectory.Create<IPropertiesPoco>( p => p.PropertiesJson = ToJson( properties ) );
    }
}
