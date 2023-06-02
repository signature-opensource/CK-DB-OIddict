using System.Collections.Generic;
using CK.Core;
using CK.Cris;
using CK.DB.OpenIddictSql.Entities;

namespace CK.DB.OpenIddictSql.Commands;

public interface IApplicationsCommand : ICommand<IApplicationsResult> { }

public interface IApplicationsResult : IPoco
{
    List<Application> Applications { get; set; }
}
