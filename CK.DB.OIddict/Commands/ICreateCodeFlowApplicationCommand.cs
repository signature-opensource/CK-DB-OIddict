using System.Collections.Generic;
using CK.Cris;

namespace CK.DB.OIddict.Commands
{
    public interface ICreateCodeFlowApplicationCommand : ICommand<ISimpleCrisResult>
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string DisplayName { get; set; }
        public IUriPoco RedirectUri { get; set; }

        // Optional properties
        public string? Scope { get; set; }
        public HashSet<IUriPoco>? PostLogoutRedirectUris { get; set; }
        public HashSet<IUriPoco>? RedirectUris { get; set; }
    }
}
