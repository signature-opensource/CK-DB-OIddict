using CK.Cris;

namespace CK.DB.OIddict.Commands
{
    public interface ICreateApplicationCommand : ICommand<ISimpleCrisResult>
    {
        IApplicationPoco ApplicationPoco { get; set; }
    }
}
