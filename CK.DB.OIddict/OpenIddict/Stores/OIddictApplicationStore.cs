using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.OIddict.Db;
using CK.DB.OIddict.Entities;
using CK.SqlServer;
using Dapper;
using OpenIddict.Abstractions;
using static CK.DB.OIddict.Dapper.JsonTypeConverter;

namespace CK.DB.OIddict.Stores
{
    /// <inheritdoc />
    public sealed class OIddictApplicationStore : IOpenIddictApplicationStore<Application>
    {
        private readonly ISqlCallContext _callContext;
        private readonly OpenIddictApplicationTable _applicationTable;
        private const int _actorId = 1; // OpenIddict is hardcoded admin

        public OIddictApplicationStore
        (
            ISqlCallContext callContext,
            OpenIddictApplicationTable applicationTable
        )
        {
            _callContext = callContext;
            _applicationTable = applicationTable;
        }

        /// <inheritdoc />
        public async ValueTask<long> CountAsync( CancellationToken cancellationToken )
        {
            const string sql = "select count(*) from CK.tOpenIddictApplication";
            var controller = _callContext[_applicationTable];

            return await controller.QuerySingleAsync<long>( sql );
        }

        /// <inheritdoc />
        public async ValueTask<long> CountAsync<TResult>
        (
            Func<IQueryable<Application>, IQueryable<TResult>> query,
            CancellationToken cancellationToken
        )
        {
            if( query == null ) throw new ArgumentNullException( nameof( query ) );

            const string sql = @"
select
    ApplicationId,
    ClientId,
    ClientSecret,
    ConsentType,
    DisplayName,
    DisplayNames,
    Permissions,
    PostLogoutRedirectUris,
    Properties,
    RedirectUris,
    Requirements,
    Type
from CK.tOpenIddictApplication
";
            var controller = _callContext[_applicationTable];

            var applications = await controller.QueryAsync<Application>( sql, cancellationToken: cancellationToken );
            var result = query.Invoke( applications.AsQueryable() );

            return result.Count();
        }

        /// <inheritdoc />
        public async ValueTask CreateAsync( Application application, CancellationToken cancellationToken )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );
            if( application.ApplicationId == default )
                throw new ArgumentException( nameof( application.ApplicationId ) );
            Throw.CheckNotNullOrEmptyArgument( application.ClientId );
            Throw.CheckNotNullOrEmptyArgument( application.ClientSecret );
            Throw.CheckNotNullOrEmptyArgument( application.ConsentType );
            Throw.CheckNotNullOrEmptyArgument( application.DisplayName );

            const string sql = @"
insert into CK.tOpenIddictApplication
(
    ApplicationId,
    ClientId,
    ClientSecret,
    ConsentType,
    DisplayName,
    DisplayNames,
    Permissions,
    PostLogoutRedirectUris,
    Properties,
    RedirectUris,
    Requirements,
    Type
)
values
(
    @ApplicationId,
    @ClientId,
    @ClientSecret,
    @ConsentType,
    @DisplayName,
    @DisplayNames,
    @Permissions,
    @PostLogoutRedirectUris,
    @Properties,
    @RedirectUris,
    @Requirements,
    @Type
);
";

            var controller = _callContext[_applicationTable];

            await controller.ExecuteAsync
            (
                sql,
                new
                {
                    application.ApplicationId,
                    application.ClientId,
                    application.ClientSecret,
                    application.ConsentType,
                    application.DisplayName,
                    DisplayNames = ToJson( application.DisplayNames ),
                    Permissions = ToJson( application.Permissions ),
                    PostLogoutRedirectUris = ToJson( application.PostLogoutRedirectUris ),
                    Properties = ToJson( application.Properties ),
                    RedirectUris = ToJson( application.RedirectUris ),
                    Requirements = ToJson( application.Requirements ),
                    application.Type,
                }
            );
        }

        /// <inheritdoc />
        public async ValueTask DeleteAsync( Application application, CancellationToken cancellationToken )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            var controller = _callContext[_applicationTable];

            const string sql = @"
delete from CK.tOpenIddictApplication
where ApplicationId = @ApplicationId;
";

            await controller.ExecuteAsync( sql, new { application.ApplicationId } );
        }

        /// <inheritdoc />
        public async ValueTask<Application?> FindByIdAsync
        (
            string identifier,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( identifier );

            const string sql = @"
select ApplicationId,
       ClientId,
       ClientSecret,
       ConsentType,
       DisplayName,
       DisplayNames,
       Permissions,
       PostLogoutRedirectUris,
       Properties,
       RedirectUris,
       Requirements,
       Type
from CK.tOpenIddictApplication
where ApplicationId = @ApplicationId;
";
            var controller = _callContext[_applicationTable];

            return await controller.QuerySingleOrDefaultAsync<Application>( sql, new { ApplicationId = identifier } );
        }

        /// <inheritdoc />
        public async ValueTask<Application?> FindByClientIdAsync
        (
            string identifier,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( identifier );

            const string sql = @"
select ApplicationId,
       ClientId,
       ClientSecret,
       ConsentType,
       DisplayName,
       DisplayNames,
       Permissions,
       PostLogoutRedirectUris,
       Properties,
       RedirectUris,
       Requirements,
       Type
from CK.tOpenIddictApplication
where ClientId = @ClientId;
";
            var controller = _callContext[_applicationTable];

            return await controller.QuerySingleOrDefaultAsync<Application>
            (
                sql,
                param: new { ClientId = identifier }
            );
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Application> FindByPostLogoutRedirectUriAsync
        (
            string uri,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( uri );

            const string sql = @"
select ApplicationId,
       ClientId,
       ClientSecret,
       ConsentType,
       DisplayName,
       DisplayNames,
       Permissions,
       PostLogoutRedirectUris,
       Properties,
       RedirectUris,
       Requirements,
       Type
from CK.tOpenIddictApplication
where PostLogoutRedirectUris like concat('%', @uri, '%') collate Latin1_General_100_CI_AS;
";

            var controller = _callContext[_applicationTable];

            var applications = await controller.QueryAsync<Application>
            (
                sql,
                new { uri },
                cancellationToken: cancellationToken
            );

            foreach( var application in applications )
            {
                yield return application;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Application> FindByRedirectUriAsync
        (
            string uri,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( uri );

            const string sql = @"
select ApplicationId,
       ClientId,
       ClientSecret,
       ConsentType,
       DisplayName,
       DisplayNames,
       Permissions,
       PostLogoutRedirectUris,
       Properties,
       RedirectUris,
       Requirements,
       Type
from CK.tOpenIddictApplication
where RedirectUris like concat('%', @uri, '%') collate Latin1_General_100_CI_AS;
";

            var controller = _callContext[_applicationTable];

            var applications = await controller.QueryAsync<Application>
            (
                sql,
                new { uri },
                cancellationToken: cancellationToken
            );

            foreach( var application in applications )
            {
                yield return application;
            }
        }

        /// <inheritdoc />
        public ValueTask<TResult?> GetAsync<TState, TResult>
        (
            Func<IQueryable<Application>, TState, IQueryable<TResult>> query,
            TState state,
            CancellationToken cancellationToken
        )
        {
            if( query == null ) throw new ArgumentNullException( nameof( query ) );
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ValueTask<string?> GetClientIdAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return ValueTask.FromResult( application.ClientId );
        }

        /// <inheritdoc />
        public ValueTask<string?> GetClientSecretAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return ValueTask.FromResult( application.ClientSecret );
        }

        /// <inheritdoc />
        public ValueTask<string?> GetClientTypeAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return ValueTask.FromResult( application.Type );
        }

        /// <inheritdoc />
        public ValueTask<string?> GetConsentTypeAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return ValueTask.FromResult( application.ConsentType );
        }

        /// <inheritdoc />
        public ValueTask<string?> GetDisplayNameAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return ValueTask.FromResult( application.DisplayName );
        }

        /// <inheritdoc />
        public ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return ValueTask.FromResult( (application.DisplayNames ?? new()).ToImmutableDictionary() );
        }

        /// <inheritdoc />
        public ValueTask<string?> GetIdAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return ValueTask.FromResult( application.ApplicationId.ToString() )!;
        }

        /// <inheritdoc />
        public ValueTask<ImmutableArray<string>> GetPermissionsAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return ValueTask.FromResult( (application.Permissions ?? new()).ToImmutableArray() );
        }

        /// <inheritdoc />
        public ValueTask<ImmutableArray<string>> GetPostLogoutRedirectUrisAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return ValueTask.FromResult
            (
                (application.PostLogoutRedirectUris ?? new()).Select( uri => uri.ToString() ).ToImmutableArray()
            );
        }

        /// <inheritdoc />
        public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return ValueTask.FromResult
            (
                (application.Properties ?? new()).ToImmutableDictionary()
            );
        }

        /// <inheritdoc />
        public ValueTask<ImmutableArray<string>> GetRedirectUrisAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return ValueTask.FromResult
            (
                (application.RedirectUris ?? new()).Select( uri => uri.ToString() ).ToImmutableArray()
            );
        }

        /// <inheritdoc />
        public ValueTask<ImmutableArray<string>> GetRequirementsAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return ValueTask.FromResult
            (
                (application.Requirements ?? new()).Select( uri => uri.ToString() ).ToImmutableArray()
            );
        }

        /// <inheritdoc />
        public ValueTask<Application> InstantiateAsync( CancellationToken cancellationToken )
        {
            var application = new Application
            {
                ApplicationId = Guid.NewGuid(),
            };

            return ValueTask.FromResult( application );
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Application> ListAsync
        (
            int? count,
            int? offset,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            offset ??= 0;
            var countSql = count.HasValue ? "fetch next @count rows only" : string.Empty;

            var sql = @$"
select ApplicationId,
       ClientId,
       ClientSecret,
       ConsentType,
       DisplayName,
       DisplayNames,
       Permissions,
       PostLogoutRedirectUris,
       Properties,
       RedirectUris,
       Requirements,
       Type
from CK.tOpenIddictApplication
order by ApplicationId
offset @offset rows
{countSql};
";

            var controller = _callContext[_applicationTable];

            var applications = await controller.QueryAsync<Application>
            (
                sql,
                new { count, offset },
                cancellationToken: cancellationToken
            );

            foreach( var application in applications )
            {
                yield return application;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<TResult> ListAsync<TState, TResult>
        (
            Func<IQueryable<Application>, TState, IQueryable<TResult>> query,
            TState state,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            if( query == null ) throw new ArgumentNullException( nameof( query ) );

            const string sql = @"
select ApplicationId,
       ClientId,
       ClientSecret,
       ConsentType,
       DisplayName,
       DisplayNames,
       Permissions,
       PostLogoutRedirectUris,
       Properties,
       RedirectUris,
       Requirements,
       Type
from CK.tOpenIddictApplication
";
            var controller = _callContext[_applicationTable];

            var applications = await controller.QueryAsync<Application>
            (
                sql,
                cancellationToken: cancellationToken
            );

            var applicationsFiltered = query.Invoke( applications.AsQueryable(), state );
            foreach( var application in applicationsFiltered )
            {
                yield return application;
            }
        }

        /// <inheritdoc />
        public ValueTask SetClientIdAsync
        (
            Application application,
            string? identifier,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            application.ClientId = identifier;

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetClientSecretAsync
        (
            Application application,
            string? secret,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            application.ClientSecret = secret;

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetClientTypeAsync
        (
            Application application,
            string? type,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            application.Type = type;

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetConsentTypeAsync
        (
            Application application,
            string? type,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            application.ConsentType = type;

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetDisplayNameAsync
        (
            Application application,
            string? name,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            application.DisplayName = name;

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetDisplayNamesAsync
        (
            Application application,
            ImmutableDictionary<CultureInfo, string> names,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            application.DisplayNames = names.ToDictionary( pair => pair.Key, pair => pair.Value );

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetPermissionsAsync
        (
            Application application,
            ImmutableArray<string> permissions,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            application.Permissions = permissions.ToHashSet();

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetPostLogoutRedirectUrisAsync
        (
            Application application,
            ImmutableArray<string> uris,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            application.PostLogoutRedirectUris = uris.Select( u => new Uri( u ) ).ToHashSet();

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetPropertiesAsync
        (
            Application application,
            ImmutableDictionary<string, JsonElement> properties,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            application.Properties = properties.ToDictionary( pair => pair.Key, pair => pair.Value );

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetRedirectUrisAsync
        (
            Application application,
            ImmutableArray<string> uris,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            application.RedirectUris = uris.Select( u => new Uri( u ) ).ToHashSet();

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetRequirementsAsync
        (
            Application application,
            ImmutableArray<string> requirements,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            application.Requirements = requirements.ToHashSet();
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public async ValueTask UpdateAsync( Application application, CancellationToken cancellationToken )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );
            Throw.CheckNotNullArgument( application.ApplicationId );

            const string sql = @"
update CK.tOpenIddictApplication
set
    ClientId = @ClientId,
    ClientSecret = @ClientSecret,
    ConsentType = @ConsentType,
    DisplayName = @DisplayName,
    DisplayNames = @DisplayNames,
    Permissions = @Permissions,
    PostLogoutRedirectUris = @PostLogoutRedirectUris,
    Properties = @Properties,
    RedirectUris = @RedirectUris,
    Requirements = @Requirements,
    Type = @ApplicationType
where ApplicationId = @ApplicationId
";

            var controller = _callContext[_applicationTable];
            await controller.ExecuteAsync
            (
                sql,
                new
                {
                    application.ClientId,
                    application.ClientSecret,
                    application.ConsentType,
                    application.DisplayName,
                    application.DisplayNames,
                    application.Permissions,
                    application.PostLogoutRedirectUris,
                    application.Properties,
                    application.RedirectUris,
                    application.Requirements,
                    ApplicationType = application.Type,
                    application.ApplicationId,
                }
            );
        }
    }
}
