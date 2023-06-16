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
using OpenIddict.Abstractions;
using static CK.DB.OpenIddictSql.Dapper.JsonTypeConverter;


namespace CK.DB.OpenIddictSql.Stores
{
    public sealed class OpenIddictSqlScopeStore : IOpenIddictScopeStore<Scope>
    {
        private readonly ISqlCallContext _callContext;
        private readonly OpenIddictScopeTable _scopeTable;
        private const int _actorId = 1; // OpenIddict is hardcoded admin

        public OpenIddictSqlScopeStore
        (
            ISqlCallContext callContext,
            OpenIddictScopeTable scopeTable
        )
        {
            _callContext = callContext;
            _scopeTable = scopeTable;
        }

        /// <inheritdoc />
        public async ValueTask<long> CountAsync( CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask<long> CountAsync<TResult>
        ( Func<IQueryable<Scope>, IQueryable<TResult>> query, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask CreateAsync( Scope scope, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( scope );
            Throw.CheckNotNullArgument( scope.ScopeId );
            Throw.CheckNotNullArgument( scope.ScopeName );

            await _scopeTable.CreateAsync
            (
                _callContext,
                _actorId,
                scope.ScopeId,
                scope.Description,
                ToJson( scope.Descriptions ),
                scope.DisplayName,
                ToJson( scope.DisplayNames ),
                scope.ScopeName,
                ToJson( scope.Properties ),
                ToJson( scope.Resources )
            );
        }

        /// <inheritdoc />
        public async ValueTask DeleteAsync( Scope scope, CancellationToken cancellationToken )
        {
            Throw.CheckNotNullArgument( scope );

            await _scopeTable.DestroyAsync( _callContext, _actorId, scope.ScopeId );
        }

        /// <inheritdoc />
        public async ValueTask<Scope> FindByIdAsync( string identifier, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask<Scope> FindByNameAsync( string name, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IAsyncEnumerable<Scope> FindByNamesAsync( ImmutableArray<string> names, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IAsyncEnumerable<Scope> FindByResourceAsync( string resource, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask<TResult> GetAsync<TState, TResult>
        ( Func<IQueryable<Scope>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask<string> GetDescriptionAsync( Scope scope, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask<ImmutableDictionary<CultureInfo, string>> GetDescriptionsAsync
        (
            Scope scope,
            CancellationToken cancellationToken
        )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask<string> GetDisplayNameAsync( Scope scope, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync
        (
            Scope scope,
            CancellationToken cancellationToken
        )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask<string> GetIdAsync( Scope scope, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask<string> GetNameAsync( Scope scope, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync
        (
            Scope scope,
            CancellationToken cancellationToken
        )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask<ImmutableArray<string>> GetResourcesAsync( Scope scope, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask<Scope> InstantiateAsync( CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IAsyncEnumerable<Scope> ListAsync( int? count, int? offset, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IAsyncEnumerable<TResult> ListAsync<TState, TResult>
        ( Func<IQueryable<Scope>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask SetDescriptionAsync( Scope scope, string description, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask SetDescriptionsAsync
        (
            Scope scope,
            ImmutableDictionary<CultureInfo, string> descriptions,
            CancellationToken cancellationToken
        )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask SetDisplayNameAsync( Scope scope, string name, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask SetDisplayNamesAsync
        (
            Scope scope,
            ImmutableDictionary<CultureInfo, string> names,
            CancellationToken cancellationToken
        )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask SetNameAsync( Scope scope, string name, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask SetPropertiesAsync
        (
            Scope scope,
            ImmutableDictionary<string, JsonElement> properties,
            CancellationToken cancellationToken
        )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask SetResourcesAsync
        (
            Scope scope,
            ImmutableArray<string> resources,
            CancellationToken cancellationToken
        )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask UpdateAsync( Scope scope, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }
    }
}
