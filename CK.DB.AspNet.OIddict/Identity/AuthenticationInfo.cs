using System.Collections.Generic;

namespace CK.DB.AspNet.OIddict.Identity
{
    public record AuthenticationInfo( string UserName, int UserId, Dictionary<string, string> Info );
}
