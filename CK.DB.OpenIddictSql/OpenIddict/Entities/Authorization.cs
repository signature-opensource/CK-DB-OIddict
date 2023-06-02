using System;
using System.Collections.Generic;
using System.Text.Json;
using CK.Core;
using OpenIddict.Abstractions;

namespace CK.DB.OpenIddictSql.Entities
{
    public class Authorization : OpenIddictAuthorizationDescriptor
    {
        public Guid AuthorizationId { get; set; }

        public new Dictionary<string, JsonElement> Properties
        {
            get => base.Properties;
            set
            {
                base.Properties.Clear();
                base.Properties.AddRange( value );
            }
        }
        public new HashSet<string> Scopes
        {
            get => base.Scopes;
            set
            {
                base.Scopes.Clear();
                base.Scopes.AddRange( value );
            }
        }
    }
}
