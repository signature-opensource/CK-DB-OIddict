using System;
using CK.Cris;

namespace CK.DB.OIddict.Commands
{
    public interface IGetApplicationCommand : ICommand<IApplicationPoco>
    {
        /// <summary>
        /// If set, ClientId is ignored and ApplicationId is used to fetch the application.
        /// </summary>
        Guid? ApplicationId { get; set; }
        /// <summary>
        /// If ApplicationId is not set, ClientId is used to fetch the application.
        /// </summary>
        string? ClientId { get; set; }
    }
}
