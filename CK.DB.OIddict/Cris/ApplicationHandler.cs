using System.Collections.Generic;
using System.Threading.Tasks;
using CK.Core;
using CK.Cris;
using CK.DB.OIddict.Commands;
using CK.DB.OIddict.Entities;
using OpenIddict.Abstractions;

namespace CK.DB.OIddict.Cris
{
    public class ApplicationHandler : IAutoService
    {
        [CommandHandler]
        public async Task<IGetApplicationsResult> GetApplicationsAsync
        (
            IGetApplicationsCommand command,
            PocoDirectory pocoDirectory,
            IOpenIddictApplicationManager applicationManager
        )
        {
            var applications = new List<Application>();

            await foreach( var application in applicationManager.ListAsync() )
                applications.Add( (Application)application );

            var applicationResult = pocoDirectory.Create<IGetApplicationsResult>
            (
                r => r.Applications = applications
            );

            return applicationResult;
        }

        [CommandHandler]
        public async Task<ISimpleCrisResult> CreateApplicationAsync
        (
            ICreateApplicationCommand command,
            PocoDirectory pocoDirectory,
            IOpenIddictApplicationManager applicationManager
        )
        {
            Throw.CheckNotNullArgument( command.Descriptor.ClientId );

            var client = await applicationManager.FindByClientIdAsync( command.Descriptor.ClientId );

            if( client != null )
                return pocoDirectory.Failure( "Application already exists." );

            await applicationManager.CreateAsync( command.Descriptor );

            return pocoDirectory.Success();
        }
    }
}
