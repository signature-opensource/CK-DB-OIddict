using System;
using System.Collections.Generic;
using System.Text.Json;
using static CK.DB.OIddict.Dapper.JsonTypeConverter;

namespace CK.DB.OIddict.Entities
{
    public class Authorization
    {
        public Guid AuthorizationId { get; init; }

        /// <summary>
        /// Gets or sets the application identifier associated with the authorization.
        /// </summary>
        public Guid? ApplicationId { get; set; }

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

        public static Authorization? FromDbModel( AuthorizationDbModel? dbModel )
        {
            if( dbModel is null ) return null;

            var authorization = new Authorization
            {
                AuthorizationId = dbModel.AuthorizationId,
                ApplicationId = dbModel.ApplicationId,
                CreationDate = dbModel.CreationDate,
                Properties = FromJson<Dictionary<string, JsonElement>>( dbModel.Properties ),
                Scopes = FromJson<HashSet<string>>( dbModel.Scopes ),
                Status = dbModel.Status,
                Subject = dbModel.Subject,
                Type = dbModel.Type,
            };

            return authorization;
        }
    }

    public class AuthorizationDbModel
    {
        public Guid AuthorizationId { get; set; }
        public Guid? ApplicationId { get; set; }
        public DateTimeOffset? CreationDate { get; set; }
        public string? Properties { get; set; }
        public string? Scopes { get; set; }
        public string? Status { get; set; }
        public string? Subject { get; set; }
        public string? Type { get; set; }
    }
}
