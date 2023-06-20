using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CK.DB.OIddict.Entities
{
    public class Authorization
    {
        public Guid AuthorizationId { get; init; }

        /// <summary>
        /// Gets or sets the application identifier associated with the authorization.
        /// </summary>
        public string? ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the creation date associated with the authorization.
        /// </summary>
        public DateTimeOffset? CreationDate { get; set; }

        /// <summary>
        /// Gets the additional properties associated with the authorization.
        /// </summary>
        public Dictionary<string, JsonElement>? Properties { get; set; }

        /// <summary>Gets the scopes associated with the authorization.</summary>
        public HashSet<string>? Scopes { get; set; }

        /// <summary>
        /// Gets or sets the status associated with the authorization.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets the subject associated with the authorization.
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>Gets or sets the type of the authorization.</summary>
        public string? Type { get; set; }
    }
}
