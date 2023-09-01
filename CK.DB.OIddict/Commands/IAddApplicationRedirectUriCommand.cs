using System;
using CK.Cris;

namespace CK.DB.OIddict.Commands
{
    public interface IAddApplicationRedirectUriCommand : ICommand<ISimpleCrisResult>
    {
        public Guid ApplicationId { get; set; }
        public string RedirectUri { get; set; }
    }
}
