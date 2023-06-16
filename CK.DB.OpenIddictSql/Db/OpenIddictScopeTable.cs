using System;
using System.Threading.Tasks;
using CK.Core;
using CK.SqlServer;

namespace CK.DB.OpenIddictSql.Db
{
    [SqlTable( "tOpenIddictScope", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    public abstract class OpenIddictScopeTable : SqlTable
    {
        [SqlProcedure( "sOpenIddictScopeCreate" )]
        public abstract Task CreateAsync
        (
            ISqlCallContext c,
            int actorId,
            Guid scopeId,
            string description,
            string descriptions,
            string displayName,
            string displayNames,
            string scopeName,
            string properties,
            string resources
        );

        [SqlProcedure( "sOpenIddictScopeDestroy" )]
        public abstract Task DestroyAsync
        (
            ISqlCallContext c,
            int actorId,
            Guid scopeId
        );

    }
}
