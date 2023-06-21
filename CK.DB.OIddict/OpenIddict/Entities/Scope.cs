using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using static CK.DB.OIddict.Dapper.JsonTypeConverter;

namespace CK.DB.OIddict.Entities
{
    public class Scope
    {
        public Guid ScopeId { get; init; }

        /// <summary>
        /// Gets or sets the description associated with the scope.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets the localized descriptions associated with the scope.
        /// </summary>
        public Dictionary<CultureInfo, string>? Descriptions { get; set; }

        /// <summary>
        /// Gets or sets the display name associated with the scope.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets the localized display names associated with the scope.
        /// </summary>
        public Dictionary<CultureInfo, string>? DisplayNames { get; set; }

        /// <summary>
        /// Gets or sets the unique name associated with the scope.
        /// </summary>
        public string? ScopeName { get; set; }

        /// <summary>
        /// Gets the additional properties associated with the scope.
        /// </summary>
        public Dictionary<string, JsonElement>? Properties { get; set; }

        /// <summary>Gets the resources associated with the scope.</summary>
        public HashSet<string>? Resources { get; set; }

        public static Scope? FromDbModel( ScopeDbModel? dbModel )
        {
            if( dbModel is null ) return null;

            var scope = new Scope
            {
                ScopeId = dbModel.ScopeId,
                Description = dbModel.Description,
                Descriptions = FromJson<Dictionary<CultureInfo, string>>( dbModel.Descriptions ),
                DisplayName = dbModel.DisplayName,
                DisplayNames = FromJson<Dictionary<CultureInfo, string>>( dbModel.DisplayNames ),
                ScopeName = dbModel.ScopeName,
                Properties = FromJson<Dictionary<string, JsonElement>>( dbModel.Properties ),
                Resources = FromJson<HashSet<string>>( dbModel.Resources ),
            };

            return scope;
        }
    }

    public class ScopeDbModel
    {
        public Guid ScopeId { get; init; }
        public string? Description { get; init; }
        public string? Descriptions { get; init; }
        public string? DisplayName { get; init; }
        public string? DisplayNames { get; init; }
        public string? ScopeName { get; init; }
        public string? Properties { get; init; }
        public string? Resources { get; init; }
    }
}
