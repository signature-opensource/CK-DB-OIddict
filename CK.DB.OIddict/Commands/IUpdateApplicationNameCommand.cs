using System;
using CK.Cris;

namespace CK.DB.OIddict.Commands
{
    public interface IUpdateApplicationNameCommand : ICommand<ISimpleCrisResult>
    {
        public Guid ApplicationId { get; set; }
        public string DisplayName { get; set; }
    }
}
