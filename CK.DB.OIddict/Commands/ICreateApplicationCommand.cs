using CK.Cris;
using OpenIddict.Abstractions;

namespace CK.DB.OIddict.Commands
{
    public interface ICreateApplicationCommand : ICommand<ISimpleCrisResult>
    {
        OpenIddictApplicationDescriptor Descriptor { get; set; }
    }
}
