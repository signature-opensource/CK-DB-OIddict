using System;
using CK.Cris;

namespace CK.DB.OIddict.Commands
{
    public interface IDestroyApplicationCommand : ICommand<ISimpleCrisResult>
    {
        public Guid ApplicationId { get; set; }
    }
}
