using System;
using System.Collections.Generic;
using System.Text.Json;

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

        private DateTimeOffset _expirationDate;

        /// <summary>
        /// Gets or sets the expiration date associated with the token.
        /// </summary>
        public DateTimeOffset? ExpirationDate
        {
            get => _expirationDate;
            set
            {
                if( value != null ) _expirationDate = new DateTimeOffset( value.Value.DateTime, TimeSpan.Zero );
            }
        }

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
    }
}
