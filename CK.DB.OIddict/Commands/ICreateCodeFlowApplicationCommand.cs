using System.Collections.Generic;
using CK.Cris;

namespace CK.DB.OIddict.Commands
{
    public interface ICreateCodeFlowApplicationCommand : ICommand<ISimpleCrisResult>
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string DisplayName { get; set; }
        public string RedirectUri { get; set; }

        // Optional properties
        public string? Scope { get; set; }
        public HashSet<string>? PostLogoutRedirectUris { get; set; }
        public HashSet<string>? RedirectUris { get; set; }
    }
}
