using System;
using System.Threading.Tasks;
using CK.Core;
using CK.SqlServer;

namespace CK.DB.OpenIddictSql.Db
{
    [SqlTable( "tOpenIddictApplication", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    public abstract class OpenIddictApplicationTable : SqlTable
    {
        [SqlProcedure( "sOpenIddictApplicationCreate" )]
        public abstract Task CreateAsync
        (
            ISqlCallContext c,
            int actorId,
            Guid applicationId,
            string clientId,
            string clientSecret,
            string consentType,
            string displayName,
            string? displayNames = default,
            string? permissions = default,
            string? postLogoutRedirectUris = default,
            string? properties = default,
            string? redirectUris = default,
            string? requirements = default,
            string? type = default
        );

        [SqlProcedure( "sOpenIddictApplicationDestroy" )]
        public abstract Task DestroyAsync
        (
            ISqlCallContext c,
            int actorId,
            Guid applicationId
        );
    }
}
