using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

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
    }
}
