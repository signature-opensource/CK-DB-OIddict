using System;
using System.Threading.Tasks;
using CK.Core;
using CK.SqlServer;

namespace CK.DB.OpenIddictSql.Db
{
    [SqlTable( "tOpenIddictApplication", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    public abstract class OpenIddictApplicationTable : SqlTable { }
}
