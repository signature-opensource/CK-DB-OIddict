using CK.Core;

namespace CK.DB.OIddict.Db
{
    [SqlTable( "tOIddictToken", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    public abstract class OIddictTokenTable : SqlTable { }
}
