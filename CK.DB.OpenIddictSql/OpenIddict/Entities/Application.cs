using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using CK.Core;
using OpenIddict.Abstractions;

namespace CK.DB.OpenIddictSql.Entities
{
    public class Application : OpenIddictApplicationDescriptor
    {
        public Guid ApplicationId { get; set; }

        public new HashSet<string> Permissions
        {
            get => base.Permissions;
            set
            {
                base.Permissions.Clear();
                base.Permissions.AddRange( value );
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

        public new HashSet<Uri> PostLogoutRedirectUris
        {
            get => base.PostLogoutRedirectUris;
            set
            {
                base.PostLogoutRedirectUris.Clear();
                base.PostLogoutRedirectUris.AddRange( value );
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

        public new HashSet<Uri> RedirectUris
        {
            get => base.RedirectUris;
            set
            {
                base.RedirectUris.Clear();
                base.RedirectUris.AddRange( value );
            }
        }

        public new HashSet<string> Requirements
        {
            get => base.Requirements;
            set
            {
                base.Requirements.Clear();
                base.Requirements.AddRange( value );
            }
        }
    }
}
