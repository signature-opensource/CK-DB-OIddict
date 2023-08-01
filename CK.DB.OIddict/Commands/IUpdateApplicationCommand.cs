using CK.Cris;

namespace CK.DB.OIddict.Commands
{
    public interface IUpdateApplicationCommand : ICommand<ISimpleCrisResult>
    {
        IApplicationPoco ApplicationPoco { get; set; }

    }
}
