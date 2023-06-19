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
        [InjectObject] public OpenIddictApplicationTable OpenIddictApplicationTable { get; protected set; }
        [InjectObject] public OpenIddictAuthorizationTable OpenIddictAuthorizationTable { get; protected set; }
        [InjectObject] public OpenIddictScopeTable OpenIddictScopeTable { get; protected set; }
        [InjectObject] public OpenIddictTokenTable OpenIddictTokenTable { get; protected set; }
    }
}
