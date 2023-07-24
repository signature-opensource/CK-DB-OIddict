using CK.Core;

namespace CK.DB.OIddict.Db
{
    /// <summary>
    /// Package for OpenIddict
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    [Versions( "1.0.0" )]
    public class Package : SqlPackage
    {
        [InjectObject] public OIddictApplicationTable OIddictApplicationTable { get; protected set; }
        [InjectObject] public OIddictAuthorizationTable OIddictAuthorizationTable { get; protected set; }
        [InjectObject] public OIddictScopeTable OIddictScopeTable { get; protected set; }
        [InjectObject] public OIddictTokenTable OIddictTokenTable { get; protected set; }
    }
}
