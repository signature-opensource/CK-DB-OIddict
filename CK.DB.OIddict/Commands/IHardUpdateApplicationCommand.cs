using CK.Cris;

namespace CK.DB.OIddict.Commands
{
    public interface IHardUpdateApplicationCommand : ICommand<ISimpleCrisResult>
    {
        /// <summary>
        /// All properties will be set so be aware to set all existing values else-way they will be set to null.
        /// </summary>
        IApplicationPoco ApplicationPoco { get; set; }
    }
}
