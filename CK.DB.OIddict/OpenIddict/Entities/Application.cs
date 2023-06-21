using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using static CK.DB.OIddict.Dapper.JsonTypeConverter;

namespace CK.DB.OIddict.Entities
{
    public class Application
    {
        public Guid ApplicationId { get; init; }

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
        public Dictionary<CultureInfo, string>? DisplayNames { get; set; }

        /// <summary>Gets the permissions associated with the application.</summary>
        public HashSet<string>? Permissions { get; set; }

        /// <summary>
        /// Gets the post-logout redirect URIs associated with the application.
        /// </summary>

        public HashSet<Uri>? PostLogoutRedirectUris { get; set; }

        /// <summary>
        /// Gets the additional properties associated with the application.
        /// </summary>

        public Dictionary<string, JsonElement>? Properties { get; set; }

        /// <summary>
        /// Gets the redirect URIs associated with the application.
        /// </summary>

        public HashSet<Uri>? RedirectUris { get; set; }

        /// <summary>
        /// Gets the requirements associated with the application.
        /// </summary>
        public HashSet<string>? Requirements { get; set; }

        /// <summary>
        /// Gets or sets the application type associated with the application.
        /// </summary>
        public string? Type { get; set; }

        public static Application? FromDbModel( ApplicationDbModel? dbModel )
        {
            if( dbModel is null ) return null;

            var application = new Application
            {
                ApplicationId = dbModel.ApplicationId,
                ClientId = dbModel.ClientId,
                ClientSecret = dbModel.ClientSecret,
                ConsentType = dbModel.ConsentType,
                DisplayName = dbModel.DisplayName,
                DisplayNames = FromJson<Dictionary<CultureInfo, string>>( dbModel.DisplayNames ),
                Permissions = FromJson<HashSet<string>>( dbModel.Permissions ),
                PostLogoutRedirectUris = FromJson<HashSet<Uri>>( dbModel.PostLogoutRedirectUris ),
                Properties = FromJson<Dictionary<string, JsonElement>>( dbModel.Properties ),
                RedirectUris = FromJson<HashSet<Uri>>( dbModel.RedirectUris ),
                Requirements = FromJson<HashSet<string>>( dbModel.Requirements ),
                Type = dbModel.Type,
            };

            return application;
        }
    }

    public class ApplicationDbModel
    {
        public Guid ApplicationId { get; init; }
        public string? ClientId { get; init; }
        public string? ClientSecret { get; init; }
        public string? ConsentType { get; init; }
        public string? DisplayName { get; init; }
        public string? DisplayNames { get; init; }
        public string? Permissions { get; init; }
        public string? PostLogoutRedirectUris { get; init; }
        public string? Properties { get; init; }
        public string? RedirectUris { get; init; }
        public string? Requirements { get; init; }
        public string? Type { get; init; }
    }
}
