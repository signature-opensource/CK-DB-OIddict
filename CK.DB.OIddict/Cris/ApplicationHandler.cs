using System;
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
            object? applicationBoxed = null;
            if( command.ApplicationId is not null )
                applicationBoxed = await applicationManager.FindByIdAsync( command.ApplicationId.Value.ToString() );
            else if( command.ClientId is not null )
                applicationBoxed = await applicationManager.FindByClientIdAsync( command.ClientId );

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

            var previousValues = (Application)client;

            var application = applicationPocoFactory.Create( command.ApplicationPoco );

            var shouldUpdateSecret = application.ClientSecret is not null
                                  && application.ClientSecret != previousValues.ClientSecret;

            return await TryCatchLogAsync
            (
                pocoDirectory,
                shouldUpdateSecret ? UpdateSecret : Update,
                monitor
            );

            async Task Update() => await applicationManager.UpdateAsync( application );
            async Task UpdateSecret() => await applicationManager.UpdateAsync( application, application.ClientSecret! );
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
            application.ConsentType ??= previousValues.ConsentType;
            application.DisplayName ??= previousValues.DisplayName;
            application.Type ??= previousValues.Type;

            // @formatter:off
            application.DisplayNames = MergeCollections( application.DisplayNames, previousValues.DisplayNames );
            application.Permissions = MergeCollections( application.Permissions, previousValues.Permissions );
            application.PostLogoutRedirectUris = MergeCollections( application.PostLogoutRedirectUris, previousValues.PostLogoutRedirectUris );
            application.Properties = MergeCollections( application.Properties, previousValues.Properties );
            application.RedirectUris = MergeCollections( application.RedirectUris, previousValues.RedirectUris );
            application.Requirements = MergeCollections( application.Requirements, previousValues.Requirements );
            // @formatter:on

            var shouldUpdateSecret = application.ClientSecret is not null
                                  && application.ClientSecret != previousValues.ClientSecret;

            if( shouldUpdateSecret is false ) application.ClientSecret = previousValues.ClientSecret;

            return await TryCatchLogAsync
            (
                pocoDirectory,
                shouldUpdateSecret ? UpdateSecret : Update,
                monitor
            );

            async Task Update() => await applicationManager.UpdateAsync( application );
            async Task UpdateSecret() => await applicationManager.UpdateAsync( application, application.ClientSecret! );
        }

        private static HashSet<T>? MergeCollections<T>( HashSet<T>? destination, HashSet<T>? source )
        {
            if( destination is null ) return source;
            if( source is null ) return destination;

            foreach( var s in source )
            {
                destination.Add( s );
            }

            return destination;
        }

        private static Dictionary<TKey, TValue>? MergeCollections<TKey, TValue>
        (
            Dictionary<TKey, TValue>? destination,
            Dictionary<TKey, TValue>? source
        ) where TKey : notnull
        {
            if( destination is null ) return source;
            if( source is null ) return destination;

            foreach( var (key, value) in source )
            {
                destination[key] = value;
            }

            return destination;
        }

        [CommandHandler]
        public async Task<ISimpleCrisResult> UpdateApplicationNameAsync
        (
            IUpdateApplicationNameCommand command,
            PocoDirectory pocoDirectory,
            IOpenIddictApplicationManager applicationManager,
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

        [CommandHandler]
        public async Task<ISimpleCrisResult> DestroyApplicationAsync
        (
            IDestroyApplicationCommand command,
            PocoDirectory pocoDirectory,
            IOpenIddictApplicationManager applicationManager,
            IActivityMonitor monitor
        )
        {
            var client = await applicationManager.FindByIdAsync( command.ApplicationId.ToString() );

            if( client is null )
                return pocoDirectory.Failure( "Application not found." );

            var application = (Application)client;

            return await TryCatchLogAsync
            (
                pocoDirectory,
                async () => await applicationManager.DeleteAsync( application ),
                monitor,
                $"Application {application.ApplicationId} with clientId {application.ClientId} deleted."
            );
        }

        private async Task<ISimpleCrisResult> TryCatchLogAsync
        (
            PocoDirectory pocoDirectory,
            Func<Task> action,
            IActivityMonitor monitor,
            string? successMessage = null
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

            if( successMessage is not null ) monitor.Info( successMessage );

            return pocoDirectory.Success();
        }
    }
}
