using System.Collections.Generic;
using CK.Core;
using CK.Cris;

namespace CK.DB.OIddict.Commands
{
    public interface IGetApplicationsCommand : ICommand<IGetApplicationsResult> { }

    public interface IGetApplicationsResult : IPoco
    {
        List<IApplicationPoco> Applications { get; set; }
    }
}
