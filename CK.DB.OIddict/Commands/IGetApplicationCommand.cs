using CK.Cris;

namespace CK.DB.OIddict.Commands
{
    public interface IGetApplicationCommand : ICommand<IApplicationPoco>
    {
        string ClientId { get; set; }
    }
}
