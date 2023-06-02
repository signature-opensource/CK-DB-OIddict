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
    public sealed class OpenIddictSqlTokenStore : IOpenIddictTokenStore<Token>
    {
        private readonly ISqlCallContext _callContext;
        private readonly OpenIddictTokenTable _tokenTable;
        private const int _actorId = 1; // OpenIddict is hardcoded admin

        public OpenIddictSqlTokenStore( ISqlCallContext callContext, OpenIddictTokenTable tokenTable )
        {
            _callContext = callContext;
            _tokenTable = tokenTable;
        }

        /// <inheritdoc />
        public async ValueTask<long> CountAsync( CancellationToken cancellationToken )
        {
            const string sql = "select count(*) from CK.tOpenIddictToken";
            var controller = _callContext[_tokenTable];

            return await controller.QuerySingleAsync<long>( sql );
        }

        /// <inheritdoc />
        public async ValueTask<long> CountAsync<TResult>
        (
            Func<IQueryable<Token>, IQueryable<TResult>> query,
            CancellationToken cancellationToken
        )
        {
            if( query == null ) throw new ArgumentNullException( nameof( query ) );

            const string sql = @"
select TokenId,
       ApplicationId,
       AuthorizationId,
       CreationDate,
       ExpirationDate,
       Payload,
       Properties,
       RedemptionDate,
       ReferenceId,
       Status,
       Subject,
       Type
from CK.tOpenIddictToken
";
            var controller = _callContext[_tokenTable];

            var tokens = await controller.QueryAsync<Token>( sql, cancellationToken: cancellationToken );
            var result = query.Invoke( tokens.AsQueryable() );

            return result.Count();
        }

        /// <inheritdoc />
        public async ValueTask CreateAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );
            Throw.CheckNotNullArgument( token.ApplicationId );
            Throw.CheckNotNullArgument( token.AuthorizationId );
            Throw.CheckNotNullArgument( token.CreationDate );
            Throw.CheckNotNullArgument( token.ExpirationDate );
            Throw.CheckNotNullArgument( token.Status );
            Throw.CheckNotNullArgument( token.Subject );
            Throw.CheckNotNullArgument( token.Type );

            token.TokenId = await _tokenTable.CreateAsync
            (
                _callContext,
                _actorId,
                Guid.Parse( token.ApplicationId ),
                Guid.Parse( token.AuthorizationId ),
                token.CreationDate.Value.UtcDateTime, //todo: date
                token.ExpirationDate.Value.UtcDateTime,
                token.Payload,
                ToJson( token.Properties ),
                token.RedemptionDate?.UtcDateTime,
                token.ReferenceId != null ? Guid.Parse( token.ReferenceId ) : null,
                token.Status,
                token.Subject,
                token.Type
            );
        }

        /// <inheritdoc />
        public async ValueTask DeleteAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            await _tokenTable.DestroyAsync( _callContext, _actorId, token.TokenId );
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Token> FindAsync
        (
            string subject,
            // I follow the same implementation as OpenIddictEntityFrameworkCoreTokenStore
            // that filters on ApplicationId.
            string client,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( subject );
            Throw.CheckNotNullOrWhiteSpaceArgument( client );

            const string sql = @"
select
   t.TokenId,
   t.ApplicationId,
   t.AuthorizationId,
   t.CreationDate,
   t.ExpirationDate,
   t.Payload,
   t.Properties,
   t.RedemptionDate,
   t.ReferenceId,
   t.Status,
   t.Subject,
   t.Type
from CK.tOpenIddictToken t
where t.Subject = @subject
  and t.ApplicationId = @client;
";
            var controller = _callContext[_tokenTable];

            var tokens = await controller.QueryAsync<Token>
            (
                sql,
                new { subject, client },
                cancellationToken: cancellationToken
            );

            foreach( var token in tokens.ToArray() )
            {
                yield return token;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Token> FindAsync
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
   t.TokenId,
   t.ApplicationId,
   t.AuthorizationId,
   t.CreationDate,
   t.ExpirationDate,
   t.Payload,
   t.Properties,
   t.RedemptionDate,
   t.ReferenceId,
   t.Status,
   t.Subject,
   t.Type
from CK.tOpenIddictToken t
where t.Subject = @subject
  and t.ApplicationId = @client
  and t.Status = @status;
";
            var controller = _callContext[_tokenTable];

            var tokens = await controller.QueryAsync<Token>
            (
                sql,
                new { subject, client, status },
                cancellationToken: cancellationToken
            );

            foreach( var token in tokens.ToArray() )
            {
                yield return token;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Token> FindAsync
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
   t.TokenId,
   t.ApplicationId,
   t.AuthorizationId,
   t.CreationDate,
   t.ExpirationDate,
   t.Payload,
   t.Properties,
   t.RedemptionDate,
   t.ReferenceId,
   t.Status,
   t.Subject,
   t.Type
from CK.tOpenIddictToken t
where t.Subject = @subject
  and t.ApplicationId = @client
  and t.Status = @status
  and t.type = @type;
";
            var controller = _callContext[_tokenTable];

            var tokens = await controller.QueryAsync<Token>
            (
                sql,
                new { subject, client, status, type },
                cancellationToken: cancellationToken
            );

            foreach( var token in tokens.ToArray() )
            {
                yield return token;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Token> FindByApplicationIdAsync
        (
            string identifier,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( identifier );

            const string sql = @"
select
   t.TokenId,
   t.ApplicationId,
   t.AuthorizationId,
   t.CreationDate,
   t.ExpirationDate,
   t.Payload,
   t.Properties,
   t.RedemptionDate,
   t.ReferenceId,
   t.Status,
   t.Subject,
   t.Type
from CK.tOpenIddictToken t
where t.ApplicationId = @ApplicationId
";
            var controller = _callContext[_tokenTable];

            var tokens = await controller.QueryAsync<Token>
            (
                sql,
                new { ApplicationId = identifier },
                cancellationToken: cancellationToken
            );

            foreach( var token in tokens.ToArray() )
            {
                yield return token;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Token> FindByAuthorizationIdAsync
        (
            string identifier,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( identifier );

            const string sql = @"
select
   t.TokenId,
   t.ApplicationId,
   t.AuthorizationId,
   t.CreationDate,
   t.ExpirationDate,
   t.Payload,
   t.Properties,
   t.RedemptionDate,
   t.ReferenceId,
   t.Status,
   t.Subject,
   t.Type
from CK.tOpenIddictToken t
where t.AuthorizationId = @AuthorizationId
";
            var controller = _callContext[_tokenTable];

            var tokens = await controller.QueryAsync<Token>
            (
                sql,
                new { AuthorizationId = identifier },
                cancellationToken: cancellationToken
            );

            foreach( var token in tokens.ToArray() )
            {
                yield return token;
            }
        }

        /// <inheritdoc />
        public async ValueTask<Token?> FindByIdAsync( string identifier, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( identifier );

            const string sql = @"
select
   t.TokenId,
   t.ApplicationId,
   t.AuthorizationId,
   t.CreationDate,
   t.ExpirationDate,
   t.Payload,
   t.Properties,
   t.RedemptionDate,
   t.ReferenceId,
   t.Status,
   t.Subject,
   t.Type
from CK.tOpenIddictToken t
where t.TokenId = @TokenId
";
            var controller = _callContext[_tokenTable];

            return await controller.QuerySingleOrDefaultAsync<Token>
            (
                sql,
                new { TokenId = identifier }
            );
        }

        /// <inheritdoc />
        public async ValueTask<Token?> FindByReferenceIdAsync( string identifier, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( identifier );

            const string sql = @"
select
   t.TokenId,
   t.ApplicationId,
   t.AuthorizationId,
   t.CreationDate,
   t.ExpirationDate,
   t.Payload,
   t.Properties,
   t.RedemptionDate,
   t.ReferenceId,
   t.Status,
   t.Subject,
   t.Type
from CK.tOpenIddictToken t
where t.ReferenceId = @ReferenceId
";
            var controller = _callContext[_tokenTable];

            return await controller.QuerySingleOrDefaultAsync<Token?>
            (
                sql,
                new { ReferenceId = identifier }
            );
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Token> FindBySubjectAsync( string subject, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( subject );

            const string sql = @"
select
   t.TokenId,
   t.ApplicationId,
   t.AuthorizationId,
   t.CreationDate,
   t.ExpirationDate,
   t.Payload,
   t.Properties,
   t.RedemptionDate,
   t.ReferenceId,
   t.Status,
   t.Subject,
   t.Type
from CK.tOpenIddictToken t
where t.Subject = @subject
";
            var controller = _callContext[_tokenTable];

            var tokens = await controller.QueryAsync<Token>
            (
                sql,
                new { subject },
                cancellationToken: cancellationToken
            );

            foreach( var token in tokens.ToArray() )
            {
                yield return token;
            }
        }

        /// <inheritdoc />
        public async ValueTask<string?> GetApplicationIdAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return token.ApplicationId;
        }

        /// <inheritdoc />
        public async ValueTask<TResult?> GetAsync<TState, TResult>
        (
            Func<IQueryable<Token>, TState, IQueryable<TResult>> query,
            TState state,
            CancellationToken cancellationToken
        )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask<string?> GetAuthorizationIdAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return token.AuthorizationId;
        }


        /// <inheritdoc />
        public async ValueTask<DateTimeOffset?> GetCreationDateAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return token.CreationDate;
        }

        /// <inheritdoc />
        public async ValueTask<DateTimeOffset?> GetExpirationDateAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return token.ExpirationDate;
        }

        /// <inheritdoc />
        public async ValueTask<string?> GetIdAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return token.TokenId == default ? null : token.TokenId.ToString();
        }

        /// <inheritdoc />
        public async ValueTask<string?> GetPayloadAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return token.Payload;
        }

        /// <inheritdoc />
        public async ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync
        (
            Token token,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( token );

            return token.Properties.ToImmutableDictionary();
        }

        /// <inheritdoc />
        public async ValueTask<DateTimeOffset?> GetRedemptionDateAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return token.RedemptionDate;
        }

        /// <inheritdoc />
        public async ValueTask<string?> GetReferenceIdAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return token.ReferenceId;
        }

        /// <inheritdoc />
        public async ValueTask<string?> GetStatusAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return token.Status;
        }

        /// <inheritdoc />
        public async ValueTask<string?> GetSubjectAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return token.Subject;
        }

        /// <inheritdoc />
        public async ValueTask<string?> GetTypeAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return token.Type;
        }

        /// <inheritdoc />
        public async ValueTask<Token> InstantiateAsync( CancellationToken cancellationToken )
        {
            return new Token();
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Token> ListAsync( int? count, int? offset, CancellationToken cancellationToken )
        {
            offset ??= 0;
            var countSql = count.HasValue ? "fetch next @count rows only" : string.Empty;

            var sql = $@"
select TokenId,
       ApplicationId,
       AuthorizationId,
       CreationDate,
       ExpirationDate,
       Payload,
       Properties,
       RedemptionDate,
       ReferenceId,
       Status,
       Subject,
       Type
from CK.tOpenIddictToken
order by TokenId
offset @offset rows
{countSql};
";

            var controller = _callContext[_tokenTable];

            var tokens = await controller.QueryAsync<Token>
            (
                sql,
                new { count, offset },
                cancellationToken: cancellationToken
            );

            foreach( var token in tokens.ToArray() )
            {
                yield return token;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<TResult> ListAsync<TState, TResult>
        (
            Func<IQueryable<Token>, TState, IQueryable<TResult>> query,
            TState state,
            CancellationToken cancellationToken
        )
        {
            if( query == null ) throw new ArgumentNullException( nameof( query ) );

            const string sql = @"
select TokenId,
       ApplicationId,
       AuthorizationId,
       CreationDate,
       ExpirationDate,
       Payload,
       Properties,
       RedemptionDate,
       ReferenceId,
       Status,
       Subject,
       Type
from CK.tOpenIddictToken
";
            var controller = _callContext[_tokenTable];

            var tokens = await controller.QueryAsync<Token>
            (
                sql,
                cancellationToken: cancellationToken
            );

            var tokensFiltered = query.Invoke( tokens.AsQueryable(), state );
            foreach( var token in tokensFiltered.ToArray() )
            {
                yield return token;
            }
        }

        /// <inheritdoc />
        public async ValueTask PruneAsync( DateTimeOffset threshold, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask SetApplicationIdAsync( Token token, string? identifier, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            token.ApplicationId = identifier;
        }

        /// <inheritdoc />
        public async ValueTask SetAuthorizationIdAsync
        (
            Token token,
            string? identifier,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( token );

            token.AuthorizationId = identifier;
        }

        /// <inheritdoc />
        public async ValueTask SetCreationDateAsync
        (
            Token token,
            DateTimeOffset? date,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( token );

            token.CreationDate = date;
        }

        /// <inheritdoc />
        public async ValueTask SetExpirationDateAsync
        (
            Token token,
            DateTimeOffset? date,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( token );

            token.ExpirationDate = date;
        }

        /// <inheritdoc />
        public async ValueTask SetPayloadAsync( Token token, string? payload, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            token.Payload = payload;
        }

        /// <inheritdoc />
        public async ValueTask SetPropertiesAsync
        (
            Token token,
            ImmutableDictionary<string, JsonElement> properties,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( token );

            token.Properties.Clear();
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if( properties is null ) return;

            token.Properties.AddRange( properties );
        }

        /// <inheritdoc />
        public async ValueTask SetRedemptionDateAsync
        (
            Token token,
            DateTimeOffset? date,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( token );

            token.RedemptionDate = date;
        }

        /// <inheritdoc />
        public async ValueTask SetReferenceIdAsync( Token token, string? identifier, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            token.ReferenceId = identifier;
        }

        /// <inheritdoc />
        public async ValueTask SetStatusAsync( Token token, string? status, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            token.Status = status;
        }

        /// <inheritdoc />
        public async ValueTask SetSubjectAsync( Token token, string? subject, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            token.Subject = subject;
        }

        /// <inheritdoc />
        public async ValueTask SetTypeAsync( Token token, string? type, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            token.Type = type;
        }

        /// <inheritdoc />
        public async ValueTask UpdateAsync( Token token, CancellationToken cancellationToken )
        {
            if( token == null ) throw new ArgumentNullException( nameof( token ) );
            Throw.CheckNotNullArgument( token.TokenId );

            var sql = @"
update CK.tOpenIddictToken
set ApplicationId = @ApplicationId,
    AuthorizationId = @AuthorizationId,
    CreationDate = @CreationDate,
    ExpirationDate = @ExpirationDate,
    Payload = @Payload,
    Properties = @Properties,
    RedemptionDate = @RedemptionDate,
    ReferenceId = @ReferenceId,
    Status = @Status,
    Subject = @Subject,
    Type = @TokenType
where TokenId = @TokenId
";

            var controller = _callContext[_tokenTable];
            await controller.ExecuteAsync
            (
                sql,
                new
                {
                    token.ApplicationId,
                    token.AuthorizationId,
                    token.CreationDate,
                    token.ExpirationDate,
                    token.Payload,
                    token.Properties,
                    token.RedemptionDate,
                    token.ReferenceId,
                    token.Status,
                    token.Subject,
                    TokenType = token.Type,
                    token.TokenId,
                }
            );
        }
    }
}
