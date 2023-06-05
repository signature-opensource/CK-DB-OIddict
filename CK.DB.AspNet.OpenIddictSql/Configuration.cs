namespace CK.DB.AspNet.OpenIddictSql
{
    public record Configuration
    (
        string AuthenticationScheme,
        string LoginPath
    );

    public static class ConstantsConfiguration
    {
        public const string AuthorizeUri = "connect/authorize";
        public const string LogoutUri = "connect/logout";
        public const string TokenUri = "connect/token";
        public const string UserInfoUri = "connect/userinfo";
    }
}
