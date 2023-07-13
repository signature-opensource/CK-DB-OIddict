using System;
using System.Collections.Generic;
using System.Text.Json;
using CK.DB.OIddict.Dapper;
using static CK.DB.OIddict.Dapper.JsonTypeConverter;

namespace CK.DB.OIddict.Entities
{
    public class Token
    {
        public Guid TokenId { get; init; }

        /// <summary>
        /// Gets or sets the application identifier associated with the token.
        /// </summary>
        public Guid? ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the authorization identifier associated with the token.
        /// </summary>
        public Guid? AuthorizationId { get; set; }

        /// <summary>
        /// Gets or sets the creation date associated with the token.
        /// </summary>
        public DateTimeOffset? CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the expiration date associated with the token.
        /// </summary>
        public DateTimeOffset? ExpirationDate { get; set; }

        /// <summary>Gets or sets the payload associated with the token.</summary>
        public string? Payload { get; set; }

        /// <summary>
        /// Gets the additional properties associated with the token.
        /// </summary>
        public Dictionary<string, JsonElement>? Properties { get; set; }

        /// <summary>
        /// Gets or sets the redemption date associated with the token.
        /// </summary>
        public DateTimeOffset? RedemptionDate { get; set; }

        /// <summary>
        /// Gets or sets the reference identifier associated with the token.
        /// Note: depending on the application manager used when creating it,
        /// this property may be hashed or encrypted for security reasons.
        /// </summary>
        public string? ReferenceId { get; set; }

        /// <summary>Gets or sets the status associated with the token.</summary>
        public string? Status { get; set; }

        /// <summary>Gets or sets the subject associated with the token.</summary>
        public string? Subject { get; set; }

        /// <summary>Gets or sets the token type.</summary>
        public string? Type { get; set; }

        internal static Token? FromDbModel( TokenDbModel? dbModel )
        {
            if( dbModel is null ) return null;

            var token = new Token
            {
                TokenId = dbModel.TokenId,
                ApplicationId = dbModel.ApplicationId,
                AuthorizationId = dbModel.AuthorizationId,
                CreationDate = dbModel.CreationDate?.DateTime,
                ExpirationDate = dbModel.ExpirationDate?.DateTime,
                Payload = dbModel.Payload,
                Properties = FromJson<Dictionary<string, JsonElement>>( dbModel.Properties ),
                RedemptionDate = dbModel.RedemptionDate?.DateTime,
                ReferenceId = dbModel.ReferenceId,
                Status = dbModel.Status,
                Subject = dbModel.Subject,
                Type = dbModel.Type,
            };

            return token;
        }
    }

    internal class TokenDbModel
    {
        public Guid TokenId { get; init; }
        public Guid? ApplicationId { get; init; }
        public Guid? AuthorizationId { get; init; }
        public DateTimeUtc? CreationDate { get; init; }
        public DateTimeUtc? ExpirationDate { get; set; }
        public string? Payload { get; init; }
        public string? Properties { get; init; }
        public DateTimeUtc? RedemptionDate { get; init; }
        public string? ReferenceId { get; init; }
        public string? Status { get; init; }
        public string? Subject { get; init; }
        public string? Type { get; init; }
    }
}
