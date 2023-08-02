using System;
using System.Collections.Generic;
using System.Linq;
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
            IOpenIddictApplicationManager applicationManager,
            ApplicationPocoFactory applicationPocoFactory,
            IActivityMonitor monitor
        )
        {
            var applications = new List<IApplicationPoco>();

            await foreach( var applicationBoxed in applicationManager.ListAsync() )
            {
                var applicationPoco = applicationPocoFactory.CreatePoco( (Application)applicationBoxed );

                applications.Add( applicationPoco );
            }

            var applicationResult = pocoDirectory.Create<IGetApplicationsResult>
            (
                r => r.Applications = applications
            );

            return applicationResult;
        }

        [CommandHandler]
        public async Task<IApplicationPoco> GetApplicationAsync
        (
            IGetApplicationCommand command,
            PocoDirectory pocoDirectory,
            IOpenIddictApplicationManager applicationManager,
            ApplicationPocoFactory applicationPocoFactory
        )
        {
            var applicationBoxed = await applicationManager.FindByClientIdAsync( command.ClientId );

            if( applicationBoxed is null ) return pocoDirectory.Create<IApplicationPoco>();

            var application = (Application)applicationBoxed;

            var poco = applicationPocoFactory.CreatePoco( application );

            return poco;
        }

        [CommandHandler]
        public async Task<ISimpleCrisResult> CreateApplicationAsync
        (
            ICreateApplicationCommand command,
            PocoDirectory pocoDirectory,
            IOpenIddictApplicationManager applicationManager,
            ApplicationPocoFactory applicationPocoFactory,
            IActivityMonitor monitor
        )
        {
            Throw.CheckNotNullArgument( command.ApplicationPoco.ClientId );

            var client = await applicationManager.FindByClientIdAsync( command.ApplicationPoco.ClientId );

            if( client != null )
                return pocoDirectory.Failure( "Application already exists." );

            var application = applicationPocoFactory.CreateDescriptor( command.ApplicationPoco );

            return await TryCatchLogAsync
            (
                pocoDirectory,
                async () => await applicationManager.CreateAsync( application ),
                monitor
            );
        }

        [CommandHandler]
        public async Task<ISimpleCrisResult> CreateCodeFlowApplicationAsync
        (
            ICreateCodeFlowApplicationCommand command,
            PocoDirectory pocoDirectory,
            IOpenIddictApplicationManager applicationManager,
            ApplicationPocoFactory applicationPocoFactory,
            IActivityMonitor monitor
        )
        {
            var client = await applicationManager.FindByClientIdAsync( command.ClientId );

            if( client != null ) return pocoDirectory.Failure( "Application already exists." );

            var builder = new ApplicationDescriptorBuilder( command.ClientId, command.ClientSecret )
                          .WithDisplayName( command.DisplayName )
                          .EnsureCodeDefaults()
                          .AddRedirectUri( applicationPocoFactory.CreateUri( command.RedirectUri ) );
            if( command.Scope is not null ) builder.AddScope( command.Scope );
            if( command.PostLogoutRedirectUris is not null )
                foreach( var uri in command.PostLogoutRedirectUris )
                    builder.AddPostLogoutRedirectUri( applicationPocoFactory.CreateUri( uri ) );
            if( command.RedirectUris is not null )
                foreach( var uri in command.RedirectUris )
                    builder.AddRedirectUri( applicationPocoFactory.CreateUri( uri ) );

            if( command.Scope is not null ) builder.AddScope( command.Scope );

            var descriptor = builder.Build();

            return await TryCatchLogAsync
            (
                pocoDirectory,
                async () => await applicationManager.CreateAsync( descriptor ),
                monitor
            );
        }

        [CommandHandler]
        public async Task<ISimpleCrisResult> HardUpdateApplicationAsync
        (
            IHardUpdateApplicationCommand command,
            PocoDirectory pocoDirectory,
            IOpenIddictApplicationManager applicationManager,
            ApplicationPocoFactory applicationPocoFactory,
            IActivityMonitor monitor
        )
        {
            Throw.CheckNotNullArgument( command.ApplicationPoco.ApplicationId );

            var client = await applicationManager.FindByIdAsync
            (
                command.ApplicationPoco.ApplicationId.Value.ToString()
            );

            if( client is null )
                return pocoDirectory.Failure( "Application not found." );

            var application = applicationPocoFactory.Create( command.ApplicationPoco );

            return await TryCatchLogAsync
            (
                pocoDirectory,
                async () => await applicationManager.UpdateAsync( application ),
                monitor
            );
        }

        [CommandHandler]
        public async Task<ISimpleCrisResult> SoftUpdateApplicationAsync
        (
            ISoftUpdateApplicationCommand command,
            PocoDirectory pocoDirectory,
            IOpenIddictApplicationManager applicationManager,
            ApplicationPocoFactory applicationPocoFactory,
            IActivityMonitor monitor
        )
        {
            Throw.CheckNotNullArgument( command.ApplicationPoco.ApplicationId );

            var client = await applicationManager.FindByIdAsync
            (
                command.ApplicationPoco.ApplicationId.Value.ToString()
            );

            if( client is null )
                return pocoDirectory.Failure( "Application not found." );

            var application = applicationPocoFactory.Create( command.ApplicationPoco );
            var previousValues = (Application)client;

            application.ClientId ??= previousValues.ClientId;
            application.ClientSecret ??= previousValues.ClientSecret;
            application.ConsentType ??= previousValues.ConsentType;
            application.DisplayName ??= previousValues.DisplayName;
            application.DisplayNames ??= previousValues.DisplayNames;
            application.Permissions ??= previousValues.Permissions;
            application.PostLogoutRedirectUris ??= previousValues.PostLogoutRedirectUris;
            application.Properties ??= previousValues.Properties;
            application.RedirectUris ??= previousValues.RedirectUris;
            application.Requirements ??= previousValues.Requirements;
            application.Type ??= previousValues.Type;

            return await TryCatchLogAsync
            (
                pocoDirectory,
                async () => await applicationManager.UpdateAsync( application ),
                monitor
            );
        }

        [CommandHandler]
        public async Task<ISimpleCrisResult> UpdateApplicationNameAsync
        (
            IUpdateApplicationNameCommand command,
            PocoDirectory pocoDirectory,
            IOpenIddictApplicationManager applicationManager,
            ApplicationPocoFactory applicationPocoFactory,
            IActivityMonitor monitor
        )
        {
            var client = await applicationManager.FindByIdAsync( command.ApplicationId.ToString() );

            if( client is null )
                return pocoDirectory.Failure( "Application not found." );

            var application = (Application)client;
            application.DisplayName = command.DisplayName;

            return await TryCatchLogAsync
            (
                pocoDirectory,
                async () => await applicationManager.UpdateAsync( application ),
                monitor
            );
        }

        private async Task<ISimpleCrisResult> TryCatchLogAsync
        (
            PocoDirectory pocoDirectory,
            Func<Task> action,
            IActivityMonitor monitor
        )
        {
            try
            {
                await action.Invoke();
            }
            catch( Exception e )
            {
                monitor.Error( e );
                return pocoDirectory.Failure( $"Internal error, see logs for details. UTC now: {DateTime.UtcNow}." );
            }

            return pocoDirectory.Success();
        }
    }
}
