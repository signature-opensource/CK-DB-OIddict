﻿using System;
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
        /// </summary>
        public Dictionary<ICultureInfoPoco, string>? DisplayNames { get; set; }

        /// <summary>Gets the permissions associated with the application.</summary>
        public HashSet<string>? Permissions { get; set; }

        /// <summary>
        /// Gets the post-logout redirect URIs associated with the application.
        /// </summary>
        public HashSet<IUriPoco>? PostLogoutRedirectUris { get; set; }

        /// <summary>
        /// Gets the additional properties associated with the application.
        /// </summary>
        public IPropertiesPoco? Properties { get; set; }

        /// <summary>
        /// Gets the redirect URIs associated with the application.
        /// </summary>
        public HashSet<IUriPoco>? RedirectUris { get; set; }

        /// <summary>
        /// Gets the requirements associated with the application.
        /// </summary>
        public HashSet<string>? Requirements { get; set; }

        /// <summary>
        /// Gets or sets the application type associated with the application.
        /// </summary>
        public string? Type { get; set; }
    }

    public interface ICultureInfoPoco : IPoco
    {
        string CultureName { get; set; }
    }

    public interface IUriPoco : IPoco
    {
        string Uri { get; set; }
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
        {
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

        private ICultureInfoPoco CreateCultureInfoPoco( CultureInfo cultureInfo )
            => _pocoDirectory.Create<ICultureInfoPoco>
            (
                p => p.CultureName = cultureInfo.Name
            );

        private CultureInfo CreateCultureInfo( ICultureInfoPoco poco ) => new( poco.CultureName );

        private Dictionary<ICultureInfoPoco, string>? CreateDisplayNamesPoco
        (
            Dictionary<CultureInfo, string>? displayNames
        )
            => displayNames?.ToDictionary
            (
                d => CreateCultureInfoPoco( d.Key ),
                d => d.Value
            );

        private Dictionary<CultureInfo, string>? CreateDisplayNames( Dictionary<ICultureInfoPoco, string>? poco )
            => poco?.ToDictionary
            (
                p => CreateCultureInfo( p.Key ),
                p => p.Value
            );

        private IUriPoco CreateUriPoco( Uri uri ) => _pocoDirectory.Create<IUriPoco>( p => p.Uri = uri.ToString() );

        private Uri CreateUri( IUriPoco poco ) => new( poco.Uri );

        private HashSet<IUriPoco>? CreateUrisPoco( HashSet<Uri>? uris ) => uris?.Select( CreateUriPoco ).ToHashSet();

        private HashSet<Uri>? CreateUris( HashSet<IUriPoco>? poco ) => poco?.Select( CreateUri ).ToHashSet();

        private Dictionary<string, JsonElement>? CreateProperties( IPropertiesPoco? poco )
            => FromJson<Dictionary<string, JsonElement>>( poco?.PropertiesJson );

        private IPropertiesPoco CreatePropertiesPoco( Dictionary<string, JsonElement>? properties )
            => _pocoDirectory.Create<IPropertiesPoco>( p => p.PropertiesJson = ToJson( properties ) );
    }
}
