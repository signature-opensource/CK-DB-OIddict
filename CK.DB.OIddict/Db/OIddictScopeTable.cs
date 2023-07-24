using CK.Core;

namespace CK.DB.OIddict.Db
{
    [SqlTable( "tOIddictScope", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    public abstract class OIddictScopeTable : SqlTable { }
}
