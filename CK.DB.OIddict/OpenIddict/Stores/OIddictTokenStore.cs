﻿using System;
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
using static CK.DB.OIddict.Entities.Token;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace CK.DB.OIddict.Stores
{
    internal sealed class OIddictTokenStore : IOpenIddictTokenStore<Token>
    {
        private readonly ISqlCallContext _callContext;
        private readonly OIddictTokenTable _tokenTable;

        public OIddictTokenStore( ISqlCallContext callContext, OIddictTokenTable tokenTable )
        {
            _callContext = callContext;
            _tokenTable = tokenTable;
        }

        /// <inheritdoc />
        public async ValueTask<long> CountAsync( CancellationToken cancellationToken )
        {
            const string sql = "select count(*) from CK.tOIddictToken";
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
from CK.tOIddictToken
";
            var controller = _callContext[_tokenTable];

            var tokens = await controller.QueryAsync<TokenDbModel>( sql, cancellationToken: cancellationToken );
            var queryable = tokens.Select( FromDbModel ).AsQueryable();
            var result = query.Invoke( queryable! );

            return result.Count();
        }

        /// <inheritdoc />
        public async ValueTask CreateAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );
            Throw.CheckNotNullArgument( token.TokenId );
            Throw.CheckNotNullArgument( token.ApplicationId );
            Throw.CheckNotNullArgument( token.AuthorizationId );
            Throw.CheckNotNullArgument( token.CreationDate );
            Throw.CheckNotNullArgument( token.ExpirationDate );
            Throw.CheckNotNullArgument( token.Status );
            Throw.CheckNotNullArgument( token.Subject );
            Throw.CheckNotNullArgument( token.Type );

            const string sql = @"
insert into CK.tOIddictToken
(
    TokenId,
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
)
values
(
    @TokenId,
    @ApplicationId,
    @AuthorizationId,
    @CreationDate,
    @ExpirationDate,
    @Payload,
    @Properties,
    @RedemptionDate,
    @ReferenceId,
    @Status,
    @Subject,
    @Type
);
";

            var controller = _callContext[_tokenTable];

            await controller.ExecuteAsync
            (
                sql,
                new
                {
                    token.TokenId,
                    token.ApplicationId,
                    token.AuthorizationId,
                    CreationDate = token.CreationDate.Value.UtcDateTime,
                    ExpirationDate = token.ExpirationDate.Value.UtcDateTime,
                    token.Payload,
                    Properties = ToJson( token.Properties ),
                    RedemptionDate = token.RedemptionDate?.UtcDateTime,
                    ReferenceId = (Guid?)(token.ReferenceId != null ? Guid.Parse( token.ReferenceId ) : null),
                    token.Status,
                    token.Subject,
                    token.Type,
                }
            );
        }

        /// <inheritdoc />
        public async ValueTask DeleteAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            var controller = _callContext[_tokenTable];

            var sql = @"
delete from CK.tOIddictToken
where TokenId = @TokenId;
";
            await controller.ExecuteAsync( sql, new { token.TokenId } );
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Token> FindAsync
        (
            string subject,
            // I follow the same implementation as OpenIddictEntityFrameworkCoreTokenStore
            // that filters on ApplicationId.
            string client,
            [EnumeratorCancellation] CancellationToken cancellationToken
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
from CK.tOIddictToken t
where t.Subject = @subject
  and t.ApplicationId = @client;
";
            var controller = _callContext[_tokenTable];

            var tokens = await controller.QueryAsync<TokenDbModel>
            (
                sql,
                new { subject = int.Parse( subject ), client = Guid.Parse( client ) },
                cancellationToken: cancellationToken
            );

            foreach( var token in tokens )
            {
                yield return FromDbModel( token )!;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Token> FindAsync
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
from CK.tOIddictToken t
where t.Subject = @subject
  and t.ApplicationId = @client
  and t.Status = @status;
";
            var controller = _callContext[_tokenTable];

            var tokens = await controller.QueryAsync<TokenDbModel>
            (
                sql,
                new { subject = int.Parse( subject ), client = Guid.Parse( client ), status },
                cancellationToken: cancellationToken
            );

            foreach( var token in tokens )
            {
                yield return FromDbModel( token )!;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Token> FindAsync
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
from CK.tOIddictToken t
where t.Subject = @subject
  and t.ApplicationId = @client
  and t.Status = @status
  and t.type = @type;
";
            var controller = _callContext[_tokenTable];

            var tokens = await controller.QueryAsync<TokenDbModel>
            (
                sql,
                new { subject = int.Parse( subject ), client = Guid.Parse( client ), status, type },
                cancellationToken: cancellationToken
            );

            foreach( var token in tokens )
            {
                yield return FromDbModel( token )!;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Token> FindByApplicationIdAsync
        (
            string identifier,
            [EnumeratorCancellation] CancellationToken cancellationToken
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
from CK.tOIddictToken t
where t.ApplicationId = @ApplicationId
";
            var controller = _callContext[_tokenTable];

            var tokens = await controller.QueryAsync<TokenDbModel>
            (
                sql,
                new { ApplicationId = Guid.Parse( identifier ) },
                cancellationToken: cancellationToken
            );

            foreach( var token in tokens )
            {
                yield return FromDbModel( token )!;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Token> FindByAuthorizationIdAsync
        (
            string identifier,
            [EnumeratorCancellation] CancellationToken cancellationToken
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
from CK.tOIddictToken t
where t.AuthorizationId = @AuthorizationId
";
            var controller = _callContext[_tokenTable];

            var tokens = await controller.QueryAsync<TokenDbModel>
            (
                sql,
                new { AuthorizationId = Guid.Parse( identifier ) },
                cancellationToken: cancellationToken
            );

            foreach( var token in tokens )
            {
                yield return FromDbModel( token )!;
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
from CK.tOIddictToken t
where t.TokenId = @TokenId
";
            var controller = _callContext[_tokenTable];

            var result = await controller.QuerySingleOrDefaultAsync<TokenDbModel>
            (
                sql,
                new { TokenId = Guid.Parse( identifier ) }
            );
            return FromDbModel( result );
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
from CK.tOIddictToken t
where t.ReferenceId = @ReferenceId
";
            var controller = _callContext[_tokenTable];

            var result = await controller.QuerySingleOrDefaultAsync<TokenDbModel>
            (
                sql,
                new { ReferenceId = identifier }
            );
            return FromDbModel( result );
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Token> FindBySubjectAsync
        ( string subject, [EnumeratorCancellation] CancellationToken cancellationToken )
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
from CK.tOIddictToken t
where t.Subject = @subject
";
            var controller = _callContext[_tokenTable];

            var tokens = await controller.QueryAsync<TokenDbModel>
            (
                sql,
                new { subject = int.Parse( subject ) },
                cancellationToken: cancellationToken
            );

            foreach( var token in tokens )
            {
                yield return FromDbModel( token )!;
            }
        }

        #region Get

        /// <inheritdoc />
        public ValueTask<string?> GetApplicationIdAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return ValueTask.FromResult( token.ApplicationId.ToString() );
        }

        /// <inheritdoc />
        public ValueTask<TResult?> GetAsync<TState, TResult>
        (
            Func<IQueryable<Token>, TState, IQueryable<TResult>> query,
            TState state,
            CancellationToken cancellationToken
        )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ValueTask<string?> GetAuthorizationIdAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return ValueTask.FromResult( token.AuthorizationId.ToString() );
        }


        /// <inheritdoc />
        public ValueTask<DateTimeOffset?> GetCreationDateAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return ValueTask.FromResult( token.CreationDate );
        }

        /// <inheritdoc />
        public ValueTask<DateTimeOffset?> GetExpirationDateAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return ValueTask.FromResult( token.ExpirationDate );
        }

        /// <inheritdoc />
        public ValueTask<string?> GetIdAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return ValueTask.FromResult( token.TokenId == default ? null : token.TokenId.ToString() );
        }

        /// <inheritdoc />
        public ValueTask<string?> GetPayloadAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return ValueTask.FromResult( token.Payload );
        }

        /// <inheritdoc />
        public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync
        (
            Token token,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( token );

            return ValueTask.FromResult( (token.Properties ?? new()).ToImmutableDictionary() );
        }

        /// <inheritdoc />
        public ValueTask<DateTimeOffset?> GetRedemptionDateAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return ValueTask.FromResult( token.RedemptionDate );
        }

        /// <inheritdoc />
        public ValueTask<string?> GetReferenceIdAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return ValueTask.FromResult( token.ReferenceId );
        }

        /// <inheritdoc />
        public ValueTask<string?> GetStatusAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return ValueTask.FromResult( token.Status );
        }

        /// <inheritdoc />
        public ValueTask<string?> GetSubjectAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return ValueTask.FromResult( token.Subject is null ? null : token.Subject.ToString() );
        }

        /// <inheritdoc />
        public ValueTask<string?> GetTypeAsync( Token token, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            return ValueTask.FromResult( token.Type );
        }

        #endregion

        /// <inheritdoc />
        public ValueTask<Token> InstantiateAsync( CancellationToken cancellationToken )
        {
            return ValueTask.FromResult( new Token ());
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Token> ListAsync
        ( int? count, int? offset, [EnumeratorCancellation] CancellationToken cancellationToken )
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
from CK.tOIddictToken
order by TokenId
offset @offset rows
{countSql};
";

            var controller = _callContext[_tokenTable];

            var tokens = await controller.QueryAsync<TokenDbModel>
            (
                sql,
                new { count, offset },
                cancellationToken: cancellationToken
            );

            foreach( var token in tokens )
            {
                yield return FromDbModel( token )!;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<TResult> ListAsync<TState, TResult>
        (
            Func<IQueryable<Token>, TState, IQueryable<TResult>> query,
            TState state,
            [EnumeratorCancellation] CancellationToken cancellationToken
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
from CK.tOIddictToken
";
            var controller = _callContext[_tokenTable];

            var tokens = await controller.QueryAsync<TokenDbModel>
            (
                sql,
                cancellationToken: cancellationToken
            );

            var tokensFiltered = query.Invoke( tokens.Select( FromDbModel ).AsQueryable()!, state );
            foreach( var token in tokensFiltered )
            {
                yield return token;
            }
        }

        /// <inheritdoc />
        public async ValueTask PruneAsync( DateTimeOffset threshold, CancellationToken cancellationToken )
        {
            var controller = _callContext[_tokenTable];

            var sql = $@"
delete t
from CK.tOIddictToken t
left join CK.tOIddictAuthorization a on a.AuthorizationId = t.AuthorizationId
where t.CreationDate < @threshold
    and
    (
        (t.Status not in ('{Statuses.Inactive}', '{Statuses.Valid}'))
     or  (a.Status is not null and a.Status != '{Statuses.Valid}')
     or  t.ExpirationDate < GetUtcDate()
    )
";

            await controller.ExecuteAsync( sql, new { threshold } );
        }

        #region Set

        /// <inheritdoc />
        public ValueTask SetApplicationIdAsync( Token token, string? identifier, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            token.ApplicationId = identifier is null ? null : Guid.Parse( identifier );

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetAuthorizationIdAsync
        (
            Token token,
            string? identifier,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( token );

            token.AuthorizationId = identifier is null ? null : Guid.Parse( identifier );

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetCreationDateAsync
        (
            Token token,
            DateTimeOffset? date,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( token );

            token.CreationDate = date;

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetExpirationDateAsync
        (
            Token token,
            DateTimeOffset? date,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( token );

            token.ExpirationDate = date;

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetPayloadAsync( Token token, string? payload, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            token.Payload = payload;

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetPropertiesAsync
        (
            Token token,
            ImmutableDictionary<string, JsonElement> properties,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( token );

            token.Properties = properties.ToDictionary( pair => pair.Key, pair => pair.Value );

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetRedemptionDateAsync
        (
            Token token,
            DateTimeOffset? date,
            CancellationToken cancellationToken
        )
        {
            Throw.CheckNotNullArgument( token );

            token.RedemptionDate = date;

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetReferenceIdAsync( Token token, string? identifier, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            token.ReferenceId = identifier;

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetStatusAsync( Token token, string? status, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            token.Status = status;

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetSubjectAsync( Token token, string? subject, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            token.Subject = subject == null ? null : int.Parse( subject );

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask SetTypeAsync( Token token, string? type, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( token );

            token.Type = type;

            return ValueTask.CompletedTask;
        }

        #endregion

        /// <inheritdoc />
        public async ValueTask UpdateAsync( Token token, CancellationToken cancellationToken )
        {
            if( token == null ) throw new ArgumentNullException( nameof( token ) );
            Throw.CheckNotNullArgument( token.TokenId );

            var sql = @"
update CK.tOIddictToken
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
                    Properties = ToJson( token.Properties ),
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
