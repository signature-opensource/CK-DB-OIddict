using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

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
    }
}
