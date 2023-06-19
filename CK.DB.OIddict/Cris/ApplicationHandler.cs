using System.Collections.Generic;
using System.Threading.Tasks;
using CK.Core;
using CK.Cris;
using CK.DB.OIddict.Commands;
using CK.DB.OIddict.Entities;
using CK.SqlServer;
using OpenIddict.Abstractions;

namespace CK.DB.OIddict.Cris
{
    public class ApplicationHandler : IAutoService
    {
        [CommandHandler]
        public async Task<IApplicationsResult> GetApplicationsAsync
        (
            ISqlCallContext sqlContext,
            IApplicationsCommand command,
            PocoDirectory pocoDirectory,
            IOpenIddictApplicationManager applicationManager
        )
        {
            var applications = new List<Application>();

            await foreach( var application in applicationManager.ListAsync() )
                applications.Add( (Application)application );

            var applicationResult = pocoDirectory.Create<IApplicationsResult>
            (
                r => r.Applications = applications
            );

            return applicationResult;
        }
    }
}
