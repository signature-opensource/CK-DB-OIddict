using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.OpenIddictSql.Db;
using CK.DB.OpenIddictSql.Entities;
using CK.SqlServer;
using Dapper;
using OpenIddict.Abstractions;
using static CK.DB.OpenIddictSql.Dapper.JsonTypeConverter;

namespace CK.DB.OpenIddictSql.Stores
{
    /// <inheritdoc />
    public sealed class OpenIddictSqlApplicationStore : IOpenIddictApplicationStore<Application>
    {
        private readonly ISqlCallContext _callContext;
        private readonly OpenIddictApplicationTable _applicationTable;
        private const int _actorId = 1; // OpenIddict is hardcoded admin

        public OpenIddictSqlApplicationStore
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
            Throw.CheckNotNullOrEmptyArgument( application.ClientId );
            Throw.CheckNotNullOrEmptyArgument( application.ClientSecret );
            Throw.CheckNotNullOrEmptyArgument( application.ConsentType );
            Throw.CheckNotNullOrEmptyArgument( application.DisplayName );

            application.ApplicationId = await _applicationTable.CreateAsync
            (
                _callContext,
                _actorId,
                application.ClientId,
                application.ClientSecret,
                application.ConsentType,
                application.DisplayName,
                ToJson( application.DisplayNames ),
                ToJson( application.Permissions ),
                ToJson( application.PostLogoutRedirectUris ),
                ToJson( application.Properties ),
                ToJson( application.RedirectUris ),
                ToJson( application.Requirements ),
                application.Type
            );
        }

        /// <inheritdoc />
        public async ValueTask DeleteAsync( Application application, CancellationToken cancellationToken )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            await _applicationTable.DestroyAsync
            (
                _callContext,
                _actorId,
                application.ApplicationId
            );
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
            CancellationToken cancellationToken
        )
        {
            throw new NotImplementedException( "Missing: Implement a sql json query on uri" );
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
where PostLogoutRedirectUris like '%@uri%' collate Latin1_General_100_CI_AS;
";

            var controller = _callContext[_applicationTable];

            var applications = await controller.QueryAsync<Application>
            (
                sql,
                new { uri },
                cancellationToken: cancellationToken
            );

            // I'm not sure if there is any point to populate the IAsyncEnumerable directly from database.
            foreach( var application in applications.ToArray() )
            {
                yield return application;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Application> FindByRedirectUriAsync
        (
            string uri,
            CancellationToken cancellationToken
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
where RedirectUris like '%@uri%' collate Latin1_General_100_CI_AS;
";
            //TODO: Query json out of uris
            var controller = _callContext[_applicationTable];

            var applications = await controller.QueryAsync<Application>
            (
                sql,
                new { uri },
                cancellationToken: cancellationToken
            );

            // I'm not sure if there is any point to populate the IAsyncEnumerable directly from database.
            foreach( var application in applications )
            {
                yield return application;
            }
        }

        /// <inheritdoc />
        public async ValueTask<TResult> GetAsync<TState, TResult>
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
        public async ValueTask<string?> GetClientIdAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return application.ClientId;
        }

        /// <inheritdoc />
        public async ValueTask<string?> GetClientSecretAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return application.ClientSecret;
        }

        /// <inheritdoc />
        public async ValueTask<string?> GetClientTypeAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return application.Type;
        }

        /// <inheritdoc />
        public async ValueTask<string?> GetConsentTypeAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return application.ConsentType;
        }

        /// <inheritdoc />
        public async ValueTask<string?> GetDisplayNameAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return application.DisplayName;
        }

        /// <inheritdoc />
        public async ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return application.DisplayNames.ToImmutableDictionary();
        }

        /// <inheritdoc />
        public async ValueTask<string?> GetIdAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return application.ApplicationId.ToString();
        }

        /// <inheritdoc />
        public async ValueTask<ImmutableArray<string>> GetPermissionsAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return application.Permissions.ToImmutableArray();
        }

        /// <inheritdoc />
        public async ValueTask<ImmutableArray<string>> GetPostLogoutRedirectUrisAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return application.PostLogoutRedirectUris.Select( uri => uri.ToString() ).ToImmutableArray();
        }

        /// <inheritdoc />
        public async ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return application.Properties.ToImmutableDictionary();
        }

        /// <inheritdoc />
        public async ValueTask<ImmutableArray<string>> GetRedirectUrisAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return application.RedirectUris.Select( uri => uri.ToString() ).ToImmutableArray();
        }

        /// <inheritdoc />
        public async ValueTask<ImmutableArray<string>> GetRequirementsAsync
        (
            Application application,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            return application.Requirements.Select( uri => uri.ToString() ).ToImmutableArray();
        }

        /// <inheritdoc />
        public async ValueTask<Application> InstantiateAsync( CancellationToken cancellationToken )
        {
            var application = new Application
            {
                // ApplicationId = Guid.NewGuid(), //TODO: wrong ? I set it on Create SP.
            };

            return application;
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Application> ListAsync
        (
            int? count,
            int? offset,
            CancellationToken cancellationToken
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

            foreach( var application in applications.ToArray() )
            {
                yield return application;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<TResult> ListAsync<TState, TResult>
        (
            Func<IQueryable<Application>, TState, IQueryable<TResult>> query,
            TState state,
            CancellationToken cancellationToken
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
            foreach( var application in applicationsFiltered.ToArray() )
            {
                yield return application;
            }
        }

        /// <inheritdoc />
        public async ValueTask SetClientIdAsync
        (
            Application application,
            string? identifier,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );
            application.ClientId = identifier;
        }

        /// <inheritdoc />
        public async ValueTask SetClientSecretAsync
        (
            Application application,
            string? secret,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );
            application.ClientSecret = secret;
        }

        /// <inheritdoc />
        public async ValueTask SetClientTypeAsync
        (
            Application application,
            string? type,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );
            application.Type = type;
        }

        /// <inheritdoc />
        public async ValueTask SetConsentTypeAsync
        (
            Application application,
            string? type,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );
            application.ConsentType = type;
        }

        /// <inheritdoc />
        public async ValueTask SetDisplayNameAsync
        (
            Application application,
            string? name,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            application.DisplayName = name;
        }

        /// <inheritdoc />
        public async ValueTask SetDisplayNamesAsync
        (
            Application application,
            ImmutableDictionary<CultureInfo, string> names,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            application.DisplayNames.Clear();
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if( names is null ) return;

            application.DisplayNames.AddRange( names );
        }

        /// <inheritdoc />
        public async ValueTask SetPermissionsAsync
        (
            Application application,
            ImmutableArray<string> permissions,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            application.Permissions.Clear();
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if( permissions == null ) return;

            application.Permissions.AddRange( permissions );
        }

        /// <inheritdoc />
        public async ValueTask SetPostLogoutRedirectUrisAsync
        (
            Application application,
            ImmutableArray<string> uris,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            application.PostLogoutRedirectUris.Clear();
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if( uris == null ) return;

            application.PostLogoutRedirectUris.AddRange( uris.Select( u => new Uri( u ) ) );
        }

        /// <inheritdoc />
        public async ValueTask SetPropertiesAsync
        (
            Application application,
            ImmutableDictionary<string, JsonElement> properties,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            application.Properties.Clear();
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if( properties is null ) return;

            application.Properties.AddRange( properties );
        }

        /// <inheritdoc />
        public async ValueTask SetRedirectUrisAsync
        (
            Application application,
            ImmutableArray<string> uris,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            application.RedirectUris.Clear();
            if( uris == null ) return;

            application.RedirectUris.AddRange( uris.Select( u => new Uri( u ) ) );
        }

        /// <inheritdoc />
        public async ValueTask SetRequirementsAsync
        (
            Application application,
            ImmutableArray<string> requirements,
            CancellationToken cancellationToken
        )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );

            application.Requirements.Clear();
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if( requirements == null ) return;

            application.Requirements.AddRange( requirements );
        }

        /// <inheritdoc />
        public async ValueTask UpdateAsync( Application application, CancellationToken cancellationToken )
        {
            if( application == null ) throw new ArgumentNullException( nameof( application ) );
            Throw.CheckNotNullArgument( application.ApplicationId );

            var sql = @"
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
