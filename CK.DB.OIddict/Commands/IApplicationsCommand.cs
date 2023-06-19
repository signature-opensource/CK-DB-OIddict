using System.Collections.Generic;
using CK.Core;
using CK.Cris;
using CK.DB.OIddict.Entities;

namespace CK.DB.OIddict.Commands;

public interface IApplicationsCommand : ICommand<IApplicationsResult> { }

public interface IApplicationsResult : IPoco
{
    List<Application> Applications { get; set; }
}
