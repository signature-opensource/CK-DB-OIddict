using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public sealed class OpenIddictSqlAuthorizationStore : IOpenIddictAuthorizationStore<Authorization>
    {
        private readonly ISqlCallContext _callContext;
        private readonly OpenIddictAuthorizationTable _authorizationTable;
        private const int _actorId = 1; // OpenIddict is hardcoded admin

        public OpenIddictSqlAuthorizationStore
        (
            ISqlCallContext callContext,
            OpenIddictAuthorizationTable authorizationTable
        )
        {
            _callContext = callContext;
            _authorizationTable = authorizationTable;
        }

        /// <inheritdoc />
        public async ValueTask<long> CountAsync( CancellationToken cancellationToken )
        {
            const string sql = "select count(*) from CK.tOpenIddictAuthorization";
            var controller = _callContext[_authorizationTable];

            return await controller.QuerySingleAsync<long>( sql );
        }

        /// <inheritdoc />
        public async ValueTask<long> CountAsync<TResult>
        (
            Func<IQueryable<Authorization>, IQueryable<TResult>> query,
            CancellationToken cancellationToken
        )
        {
            if( query == null ) throw new ArgumentNullException( nameof( query ) );

            const string sql = @"
select
    AuthorizationId,
    ApplicationId,
    CreationDate,
    Properties,
    Scopes,
    Status,
    Subject,
    Type
from CK.tOpenIddictAuthorization
";
            var controller = _callContext[_authorizationTable];

            var applications = await controller.QueryAsync<Authorization>( sql, cancellationToken: cancellationToken );
            var result = query.Invoke( applications.AsQueryable() );

            return result.Count();
        }

        /// <inheritdoc />
        public async ValueTask CreateAsync( Authorization authorization, CancellationToken cancellationToken )
        {
            if( authorization == null ) throw new ArgumentNullException( nameof( authorization ) );
            Throw.CheckNotNullArgument( authorization.CreationDate );
            Throw.CheckNotNullOrWhiteSpaceArgument( authorization.ApplicationId );
            Throw.CheckNotNullOrWhiteSpaceArgument( authorization.Status );
            Throw.CheckNotNullOrWhiteSpaceArgument( authorization.Subject );
            Throw.CheckNotNullOrWhiteSpaceArgument( authorization.Type );

            authorization.AuthorizationId = await _authorizationTable.CreateAsync
            (
                _callContext,
                _actorId,
                Guid.Parse( authorization.ApplicationId ),
                authorization.CreationDate.Value.UtcDateTime,
                ToJson( authorization.Properties ),
                ToJson( authorization.Scopes ),
                authorization.Status,
                authorization.Subject,
                authorization.Type
            );
        }

        /// <inheritdoc />
        public async ValueTask DeleteAsync( Authorization authorization, CancellationToken cancellationToken )
        {
            if( authorization == null ) throw new ArgumentNullException( nameof( authorization ) );

            await _authorizationTable.DestroyAsync
            (
                _callContext,
                _actorId,
                authorization.AuthorizationId
            );
        }

        /// <inheritdoc />
        /// <param name="client">The client applicationId associated with the authorization.</param>
        public async IAsyncEnumerable<Authorization> FindAsync
        (
            string subject,
            // I follow the same implementation as OpenIddictEntityFrameworkCoreAuthorizationStore
            // that filters on ApplicationId.
            string client,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( subject );
            Throw.CheckNotNullOrWhiteSpaceArgument( client );

            const string sql = @"
select
    auth.AuthorizationId,
    auth.ApplicationId,
    auth.CreationDate,
    auth.Properties,
    auth.Scopes,
    auth.Status,
    auth.Subject,
    auth.Type
from CK.tOpenIddictAuthorization auth
join CK.tOpenIddictApplication app on app.ApplicationId = auth.ApplicationId
where auth.Subject = @subject
  and app.ApplicationId = @client;
";
            var controller = _callContext[_authorizationTable];

            var authorizations = await controller.QueryAsync<Authorization>
            (
                sql,
                new { subject, client },
                cancellationToken: cancellationToken
            );

            foreach( var authorization in authorizations.ToArray() )
            {
                yield return authorization;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Authorization> FindAsync
        (
            string subject,
            string client,
            string status,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( subject );
            Throw.CheckNotNullOrWhiteSpaceArgument( client );
            Throw.CheckNotNullOrWhiteSpaceArgument( status );

            const string sql = @"
select
    auth.AuthorizationId,
    auth.ApplicationId,
    auth.CreationDate,
    auth.Properties,
    auth.Scopes,
    auth.Status,
    auth.Subject,
    auth.Type
from CK.tOpenIddictAuthorization auth
join CK.tOpenIddictApplication app on app.ApplicationId = auth.ApplicationId
where auth.Subject = @subject
  and app.ApplicationId = @client
  and auth.Status = @status;
";
            var controller = _callContext[_authorizationTable];

            var authorizations = await controller.QueryAsync<Authorization>
            (
                sql,
                new { subject, client, status },
                cancellationToken: cancellationToken
            );

            foreach( var authorization in authorizations )
            {
                yield return authorization;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Authorization> FindAsync
        (
            string subject,
            string client,
            string status,
            string type,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( subject );
            Throw.CheckNotNullOrWhiteSpaceArgument( client );
            Throw.CheckNotNullOrWhiteSpaceArgument( status );
            Throw.CheckNotNullOrWhiteSpaceArgument( type );

            const string sql = @"
select
    auth.AuthorizationId,
    auth.ApplicationId,
    auth.CreationDate,
    auth.Properties,
    auth.Scopes,
    auth.Status,
    auth.Subject,
    auth.Type
from CK.tOpenIddictAuthorization auth
join CK.tOpenIddictApplication app on app.ApplicationId = auth.ApplicationId
where auth.Subject = @subject
  and app.ApplicationId = @client
  and auth.Status = @status
  and auth.Type = @type;
";
            var controller = _callContext[_authorizationTable];

            var authorizations = await controller.QueryAsync<Authorization>
            (
                sql,
                new { subject, client, status, type },
                cancellationToken: cancellationToken
            );

            foreach( var authorization in authorizations )
            {
                yield return authorization;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Authorization> FindAsync
        (
            string subject,
            string client,
            string status,
            string type,
            ImmutableArray<string> scopes,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( subject );
            Throw.CheckNotNullOrWhiteSpaceArgument( client );
            Throw.CheckNotNullOrWhiteSpaceArgument( status );
            Throw.CheckNotNullOrWhiteSpaceArgument( type );
            Throw.CheckNotNullArgument( scopes );

            const string sql = @"
select
    auth.AuthorizationId,
    auth.ApplicationId,
    auth.CreationDate,
    auth.Properties,
    auth.Scopes,
    auth.Status,
    auth.Subject,
    auth.Type
from CK.tOpenIddictAuthorization auth
where auth.Subject = @subject
  and auth.ApplicationId = @client
  and auth.Status = @status
  and auth.Type = @type
  and auth.Scopes = @scopes;
";

            //TODO: scopes (json)

            var controller = _callContext[_authorizationTable];

            var authorizations = await controller.QueryAsync<Authorization>
            (
                sql,
                new { subject, client, status, type, scopes },
                cancellationToken: cancellationToken
            );

            foreach( var authorization in authorizations.ToArray() )
            {
                yield return authorization;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Authorization> FindByApplicationIdAsync
        (
            string identifier,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( identifier );

            const string sql = @"
select
    auth.AuthorizationId,
    auth.ApplicationId,
    auth.CreationDate,
    auth.Properties,
    auth.Scopes,
    auth.Status,
    auth.Subject,
    auth.Type
from CK.tOpenIddictAuthorization auth
join CK.tOpenIddictApplication app on app.ApplicationId = auth.ApplicationId
where app.ApplicationId = @ApplicationId;
";
            var controller = _callContext[_authorizationTable];

            var authorizations = await controller.QueryAsync<Authorization>
            (
                sql,
                new { ApplicationId = identifier },
                cancellationToken: cancellationToken
            );

            foreach( var authorization in authorizations.ToArray() )
            {
                yield return authorization;
            }
        }

        /// <inheritdoc />
        public async ValueTask<Authorization?> FindByIdAsync( string identifier, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( identifier );

            const string sql = @"
select
    auth.AuthorizationId,
    auth.ApplicationId,
    auth.CreationDate,
    auth.Properties,
    auth.Scopes,
    auth.Status,
    auth.Subject,
    auth.Type
from CK.tOpenIddictAuthorization auth
where auth.AuthorizationId = @AuthorizationId;
";
            var controller = _callContext[_authorizationTable];

            return await controller.QuerySingleOrDefaultAsync<Authorization>
            (
                sql,
                new { AuthorizationId = identifier }
            );
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Authorization> FindBySubjectAsync
        (
            string subject,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( subject );

            const string sql = @"
select
    auth.AuthorizationId,
    auth.ApplicationId,
    auth.CreationDate,
    auth.Properties,
    auth.Scopes,
    auth.Status,
    auth.Subject,
    auth.Type
from CK.tOpenIddictAuthorization auth
where auth.Subject = @subject;
";
            var controller = _callContext[_authorizationTable];

            var authorizations = await controller.QueryAsync<Authorization>
            (
                sql,
                new { subject },
                cancellationToken: cancellationToken
            );

            foreach( var authorization in authorizations.ToArray() )
            {
                yield return authorization;
            }
        }

        /// <inheritdoc />
        public ValueTask<string?> GetApplicationIdAsync
        (
            Authorization authorization,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            return ValueTask.FromResult( authorization.ApplicationId );
        }

        /// <inheritdoc />
        public async ValueTask<TResult?> GetAsync<TState, TResult>
        (
            Func<IQueryable<Authorization>, TState, IQueryable<TResult>> query,
            TState state,
            CancellationToken cancellationToken
        )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ValueTask<DateTimeOffset?> GetCreationDateAsync
        (
            Authorization authorization,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            return ValueTask.FromResult( authorization.CreationDate );
        }

        /// <inheritdoc />
        public ValueTask<string?> GetIdAsync( Authorization authorization, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( authorization );

            return ValueTask.FromResult
            ( authorization.AuthorizationId != default ? authorization.AuthorizationId.ToString() : null );
        }

        /// <inheritdoc />
        public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync
        (
            Authorization authorization,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            return ValueTask.FromResult( authorization.Properties.ToImmutableDictionary() );
        }

        /// <inheritdoc />
        public ValueTask<ImmutableArray<string>> GetScopesAsync
        (
            Authorization authorization,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            return ValueTask.FromResult( authorization.Scopes.ToImmutableArray() );
        }

        /// <inheritdoc />
        public ValueTask<string?> GetStatusAsync( Authorization authorization, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( authorization );

            return ValueTask.FromResult( authorization.Status );
        }

        /// <inheritdoc />
        public ValueTask<string?> GetSubjectAsync( Authorization authorization, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( authorization );

            return ValueTask.FromResult( authorization.Subject );
        }

        /// <inheritdoc />
        public ValueTask<string?> GetTypeAsync( Authorization authorization, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( authorization );

            return ValueTask.FromResult( authorization.Type );
        }

        /// <inheritdoc />
        public async ValueTask<Authorization> InstantiateAsync( CancellationToken cancellationToken )
        {
            return new Authorization();
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Authorization> ListAsync
        (
            int? count,
            int? offset,
            CancellationToken cancellationToken
        )
        {
            offset ??= 0;
            var countSql = count.HasValue ? "fetch next @count rows only" : string.Empty;
            var sql = $@"
select AuthorizationId,
       ApplicationId,
       CreationDate,
       Properties,
       Scopes,
       Status,
       Subject,
       Type
from CK.tOpenIddictAuthorization
order by AuthorizationId
offset @offset rows
{countSql};
";
//TODO: handle null values for count and offset
            var controller = _callContext[_authorizationTable];

            var authorizations = await controller.QueryAsync<Authorization>
            (
                sql,
                new { count, offset },
                cancellationToken: cancellationToken
            );

            foreach( var authorization in authorizations.ToArray() )
            {
                yield return authorization;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<TResult> ListAsync<TState, TResult>
        (
            Func<IQueryable<Authorization>, TState, IQueryable<TResult>> query,
            TState state,
            CancellationToken cancellationToken
        )
        {
            if( query == null ) throw new ArgumentNullException( nameof( query ) );

            const string sql = @"
select AuthorizationId,
       ApplicationId,
       CreationDate,
       Properties,
       Scopes,
       Status,
       Subject,
       Type
from CK.tOpenIddictAuthorization
";
            var controller = _callContext[_authorizationTable];

            var authorizations = await controller.QueryAsync<Authorization>
            (
                sql,
                cancellationToken: cancellationToken
            );

            var authorizationsFiltered = query.Invoke( authorizations.AsQueryable(), state );
            foreach( var authorization in authorizationsFiltered.ToArray() )
            {
                yield return authorization;
            }
        }

        /// <inheritdoc />
        public async ValueTask PruneAsync( DateTimeOffset threshold, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask SetApplicationIdAsync
        (
            Authorization authorization,
            string? identifier,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            authorization.ApplicationId = identifier;
        }

        /// <inheritdoc />
        public async ValueTask SetCreationDateAsync
        (
            Authorization authorization,
            DateTimeOffset? date,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            authorization.CreationDate = date;
        }

        /// <inheritdoc />
        public async ValueTask SetPropertiesAsync
        (
            Authorization authorization,
            ImmutableDictionary<string, JsonElement>? properties,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            if( properties is null || properties.Any() is false )
            {
                authorization.Properties.Clear();
                return;
            }

            authorization.Properties.AddRange( properties );
        }

        /// <inheritdoc />
        public async ValueTask SetScopesAsync
        (
            Authorization authorization,
            ImmutableArray<string> scopes,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            if( scopes == default || scopes.Any() is false )
            {
                authorization.Properties.Clear();
                return;
            }

            authorization.Scopes.AddRange( scopes );
        }

        /// <inheritdoc />
        public async ValueTask SetStatusAsync
        (
            Authorization authorization,
            string? status,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            authorization.Status = status;
        }

        /// <inheritdoc />
        public async ValueTask SetSubjectAsync
        (
            Authorization authorization,
            string? subject,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            authorization.Subject = subject;
        }

        /// <inheritdoc />
        public async ValueTask SetTypeAsync
        ( Authorization authorization, string? type, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( authorization );

            authorization.Type = type;
        }

        /// <inheritdoc />
        public async ValueTask UpdateAsync( Authorization authorization, CancellationToken cancellationToken )
        {
            if( authorization == null ) throw new ArgumentNullException( nameof( authorization ) );
            Throw.CheckNotNullArgument( authorization.AuthorizationId );

            var sql = @"
update CK.tOpenIddictAuthorization
set
    ApplicationId = @ApplicationId,
    CreationDate = @CreationDate,
    Properties = @Properties,
    Scopes = @Scopes,
    Status = @Status,
    Subject = @Subject,
    Type = @AuthorizationType
where AuthorizationId = @AuthorizationId
";

            var controller = _callContext[_authorizationTable];
            await controller.ExecuteAsync
            (
                sql,
                new
                {
                    authorization.ApplicationId,
                    authorization.CreationDate,
                    authorization.Properties,
                    authorization.Scopes,
                    authorization.Status,
                    authorization.Subject,
                    AuthorizationType = authorization.Type,
                    authorization.AuthorizationId,
                }
            );
        }
    }
}
