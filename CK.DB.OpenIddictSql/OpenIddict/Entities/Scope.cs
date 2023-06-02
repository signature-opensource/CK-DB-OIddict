using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using CK.Core;
using OpenIddict.Abstractions;

namespace CK.DB.OpenIddictSql.Entities
{
    public class Scope : OpenIddictScopeDescriptor
    {
        public Guid ScopeId { get; set; }

        public string? ScopeName
        {
            get => Name;
            set => Name = value;
        }

        public new Dictionary<CultureInfo, string> Descriptions
        {
            get => base.Descriptions;
            set
            {
                base.Descriptions.Clear();
                base.Descriptions.AddRange( value );
            }
        }

        public new Dictionary<CultureInfo, string> DisplayNames
        {
            get => base.DisplayNames;
            set
            {
                base.DisplayNames.Clear();
                base.DisplayNames.AddRange( value );
            }
        }

        public new Dictionary<string, JsonElement> Properties
        {
            get => base.Properties;
            set
            {
                base.Properties.Clear();
                base.Properties.AddRange( value );
            }
        }

        public new HashSet<string> Resources
        {
            get => base.Resources;
            set
            {
                base.Resources.Clear();
                base.Resources.AddRange( value );
            }
        }
    }
}
