using System.Collections.Generic;
using CK.Core;
using CK.Cris;
using CK.DB.OIddict.Entities;

namespace CK.DB.OIddict.Commands
{
    public interface IGetApplicationsCommand : ICommand<IGetApplicationsResult> { }

    public interface IGetApplicationsResult : IPoco
    {
        List<Application> Applications { get; set; }
    }
}
