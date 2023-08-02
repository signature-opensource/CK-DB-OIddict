using CK.Cris;

namespace CK.DB.OIddict.Commands
{
    public interface ISoftUpdateApplicationCommand : ICommand<ISimpleCrisResult>
    {
        /// <summary>
        /// Only changed properties will be set. If a property is null it is then ignored.
        /// If you want to override everything, use <see cref="IHardUpdateApplicationCommand"/>.
        /// Be aware that collections are overriden.
        /// </summary>
        IApplicationPoco ApplicationPoco { get; set; }
    }
}
