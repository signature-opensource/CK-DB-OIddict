using System;
using System.Collections.Generic;
using System.Text.Json;
using CK.Core;
using OpenIddict.Abstractions;

namespace CK.DB.OpenIddictSql.Entities
{
    public class Token : OpenIddictTokenDescriptor
    {
        public Guid TokenId { get; init; }

        public new Dictionary<string, JsonElement> Properties
        {
            get => base.Properties;
            set
            {
                base.Properties.Clear();
                base.Properties.AddRange( value );
            }
        }

        public new DateTimeOffset? ExpirationDate
        {
            get => base.ExpirationDate;
            set
            {
                if( value != null ) base.ExpirationDate = new DateTimeOffset( value.Value.DateTime, TimeSpan.Zero );
            }
        }
    }
}
