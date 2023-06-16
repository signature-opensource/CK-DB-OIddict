using System;
using System.Threading.Tasks;
using CK.Core;
using CK.SqlServer;

namespace CK.DB.OpenIddictSql.Db
{
    [SqlTable( "tOpenIddictToken", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    public abstract class OpenIddictTokenTable : SqlTable
    {
        [SqlProcedure( "sOpenIddictTokenCreate" )]
        public abstract Task CreateAsync
        (
            ISqlCallContext c,
            int actorId,
            Guid tokenId,
            Guid applicationId,
            Guid authorizationId,
            DateTime creationDate,
            DateTime expirationDate,
            string? payload,
            string properties,
            DateTime? redemptionDate,
            Guid? referenceId,
            string status,
            string subject,
            string type
        );

        [SqlProcedure( "sOpenIddictTokenDestroy" )]
        public abstract Task DestroyAsync
        (
            ISqlCallContext c,
            int actorId,
            Guid tokenId
        );
    }
}
