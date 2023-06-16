using System;
using System.Threading.Tasks;
using CK.Core;
using CK.SqlServer;

namespace CK.DB.OpenIddictSql.Db
{
    [SqlTable( "tOpenIddictAuthorization", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    public abstract class OpenIddictAuthorizationTable : SqlTable
    {
        [SqlProcedure( "sOpenIddictAuthorizationCreate" )]
        public abstract Task CreateAsync
        (
            ISqlCallContext c,
            int actorId,
            Guid authorizationId,
            Guid applicationId,
            DateTime creationDate,
            string properties,
            string scopes,
            string status,
            string subject,
            string type
        );

        [SqlProcedure( "sOpenIddictAuthorizationDestroy" )]
        public abstract Task DestroyAsync
        (
            ISqlCallContext c,
            int actorId,
            Guid authorizationId
        );

    }
}
