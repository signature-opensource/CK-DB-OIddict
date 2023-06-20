using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public sealed class OIddictAuthorizationStore : IOpenIddictAuthorizationStore<Authorization>
    {
        private readonly ISqlCallContext _callContext;
        private readonly OpenIddictAuthorizationTable _authorizationTable;

        public OIddictAuthorizationStore
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
            Throw.CheckNotNullArgument( authorization.AuthorizationId );
            Throw.CheckNotNullArgument( authorization.CreationDate );
            Throw.CheckNotNullArgument( authorization.ApplicationId );
            Throw.CheckNotNullOrWhiteSpaceArgument( authorization.Status );
            Throw.CheckNotNullOrWhiteSpaceArgument( authorization.Subject );
            Throw.CheckNotNullOrWhiteSpaceArgument( authorization.Type );

            const string sql = @"
insert into CK.tOpenIddictAuthorization
(
    AuthorizationId,
    ApplicationId,
    CreationDate,
    Properties,
    Scopes,
    Status,
    Subject,
    Type
)
values
(
    @AuthorizationId,
    @ApplicationId,
    @CreationDate,
    @Properties,
    @Scopes,
    @Status,
    @Subject,
    @Type
)
";

            var controller = _callContext[_authorizationTable];

            await controller.ExecuteAsync
            (
                sql,
                new
                {
                    authorization.AuthorizationId,
                    authorization.ApplicationId,
                    CreationDate = authorization.CreationDate.Value.UtcDateTime,
                    Properties = ToJson( authorization.Properties ),
                    Scopes = ToJson( authorization.Scopes ),
                    authorization.Status,
                    authorization.Subject,
                    authorization.Type,
                }
            );
        }

        /// <inheritdoc />
        public async ValueTask DeleteAsync( Authorization authorization, CancellationToken cancellationToken )
        {
            if( authorization == null ) throw new ArgumentNullException( nameof( authorization ) );

            const string sql = @"    delete from CK.tOpenIddictAuthorization
    where AuthorizationId = @AuthorizationId;
";
            var controller = _callContext[_authorizationTable];
            await controller.ExecuteAsync( sql, new { authorization.AuthorizationId } );
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Authorization> FindAsync
        (
            string subject,
            // I follow the same implementation as OpenIddictEntityFrameworkCoreAuthorizationStore
            // that filters on ApplicationId.
            string client,
            [EnumeratorCancellation] CancellationToken cancellationToken
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
where auth.Subject = @subject
  and auth.ApplicationId = @client;
";
            var controller = _callContext[_authorizationTable];

            var authorizations = await controller.QueryAsync<Authorization>
            (
                sql,
                new { subject, client = Guid.Parse( client ) },
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
            [EnumeratorCancellation] CancellationToken cancellationToken
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
where auth.Subject = @subject
  and auth.ApplicationId = @client
  and auth.Status = @status;
";
            var controller = _callContext[_authorizationTable];

            var authorizations = await controller.QueryAsync<Authorization>
            (
                sql,
                new { subject, client = Guid.Parse( client ), status },
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
            [EnumeratorCancellation] CancellationToken cancellationToken
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
where auth.Subject = @subject
  and auth.ApplicationId = @client
  and auth.Status = @status
  and auth.Type = @type;
";
            var controller = _callContext[_authorizationTable];

            var authorizations = await controller.QueryAsync<Authorization>
            (
                sql,
                new { subject, client = Guid.Parse( client ), status, type },
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
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( subject );
            Throw.CheckNotNullOrWhiteSpaceArgument( client );
            Throw.CheckNotNullOrWhiteSpaceArgument( status );
            Throw.CheckNotNullOrWhiteSpaceArgument( type );
            Throw.CheckNotNullArgument( scopes );

            const string sql = @"
with JsonScopes(requiredScope) as
(
    select requiredScope
    from OpenJson(@scopes)
    with (requiredScope nvarchar(max) '$')
)
select
    distinct(auth.AuthorizationId),
    auth.ApplicationId,
    auth.CreationDate,
    auth.Properties,
    auth.Scopes,
    auth.Status,
    auth.Subject,
    auth.Type
from CK.tOpenIddictAuthorization auth
CROSS APPLY OpenJson(Scopes)
    WITH (scope NVARCHAR(max) '$')
join JsonScopes js on js.requiredScope = scope
where auth.Subject = @subject
  and auth.ApplicationId = @client
  and auth.Status = @status
  and auth.Type = @type;
";

            //TODO: scopes (json) => done. It could be done differently
            // with the @scope parameter as default dapper mapping instead of json.
            // used like this : In @scopes.
            // Because dapper would do something like In (@scopes1, @scopes2)
            // We still want to save as json, this could be done without custom dapper type

            var controller = _callContext[_authorizationTable];

            var authorizations = await controller.QueryAsync<Authorization>
            (
                sql,
                new { subject, client = Guid.Parse( client ), status, type, scopes = ToJson( scopes.ToHashSet() ) },
                cancellationToken: cancellationToken
            );

            foreach( var authorization in authorizations )
            {
                yield return authorization;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Authorization> FindByApplicationIdAsync
        (
            string identifier,
            [EnumeratorCancellation] CancellationToken cancellationToken
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
where auth.ApplicationId = @ApplicationId;
";
            var controller = _callContext[_authorizationTable];

            var authorizations = await controller.QueryAsync<Authorization>
            (
                sql,
                new { ApplicationId = Guid.Parse( identifier ) },
                cancellationToken: cancellationToken
            );

            foreach( var authorization in authorizations )
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
                new { AuthorizationId = Guid.Parse( identifier ) }
            );
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Authorization> FindBySubjectAsync
        (
            string subject,
            [EnumeratorCancellation] CancellationToken cancellationToken
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

            foreach( var authorization in authorizations )
            {
                yield return authorization;
            }
        }

        #region Get

        /// <inheritdoc />
        public ValueTask<string?> GetApplicationIdAsync
        (
            Authorization authorization,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            return ValueTask.FromResult( authorization.ApplicationId.ToString() );
        }

        /// <inheritdoc />
        public ValueTask<TResult?> GetAsync<TState, TResult>
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
            (
                authorization.AuthorizationId != default ? authorization.AuthorizationId.ToString() : null
            );
        }

        /// <inheritdoc />
        public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync
        (
            Authorization authorization,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            return ValueTask.FromResult( (authorization.Properties ?? new()).ToImmutableDictionary() );
        }

        /// <inheritdoc />
        public ValueTask<ImmutableArray<string>> GetScopesAsync
        (
            Authorization authorization,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            return ValueTask.FromResult( (authorization.Scopes ?? new()).ToImmutableArray() );
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

        #endregion

        /// <inheritdoc />
        public ValueTask<Authorization> InstantiateAsync( CancellationToken cancellationToken )
        {
            return ValueTask.FromResult( new Authorization { AuthorizationId = Guid.NewGuid() } );
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Authorization> ListAsync
        (
            int? count,
            int? offset,
            [EnumeratorCancellation] CancellationToken cancellationToken
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

            var controller = _callContext[_authorizationTable];

            var authorizations = await controller.QueryAsync<Authorization>
            (
                sql,
                new { count, offset },
                cancellationToken: cancellationToken
            );

            foreach( var authorization in authorizations )
            {
                yield return authorization;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<TResult> ListAsync<TState, TResult>
        (
            Func<IQueryable<Authorization>, TState, IQueryable<TResult>> query,
            TState state,
            [EnumeratorCancellation] CancellationToken cancellationToken
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
            foreach( var authorization in authorizationsFiltered )
            {
                yield return authorization;
            }
        }

        /// <inheritdoc />
        public ValueTask PruneAsync( DateTimeOffset threshold, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        #region Set

        /// <inheritdoc />
        public ValueTask SetApplicationIdAsync
        (
            Authorization authorization,
            string? identifier,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            authorization.ApplicationId = identifier is null ? null : Guid.Parse( identifier );

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetCreationDateAsync
        (
            Authorization authorization,
            DateTimeOffset? date,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            authorization.CreationDate = date;

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetPropertiesAsync
        (
            Authorization authorization,
            ImmutableDictionary<string, JsonElement>? properties,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            authorization.Properties = properties?.ToDictionary( pair => pair.Key, pair => pair.Value );

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetScopesAsync
        (
            Authorization authorization,
            ImmutableArray<string> scopes,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            authorization.Scopes = scopes.ToHashSet();

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetStatusAsync
        (
            Authorization authorization,
            string? status,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            authorization.Status = status;

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetSubjectAsync
        (
            Authorization authorization,
            string? subject,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            authorization.Subject = subject;

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetTypeAsync
        (
            Authorization authorization,
            string? type,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( authorization );

            authorization.Type = type;

            return ValueTask.CompletedTask;
        }

        #endregion

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
                    Properties = ToJson( authorization.Properties ),
                    Scopes = ToJson( authorization.Scopes ),
                    authorization.Status,
                    authorization.Subject,
                    AuthorizationType = authorization.Type,
                    authorization.AuthorizationId,
                }
            );
        }
    }
}
