namespace CK.DB.AspNet.OIddict
{
    /// <summary>
    /// Configure the authentication.
    /// </summary>
    /// <param name="AuthenticationScheme">The scheme that handle the Challenge.</param>
    /// <param name="LoginPath">If set, bypass Authentication Handler on Challenge on the provided scheme.
    /// Use it if the authenticationScheme does not support standard Challenge.</param>
    public record Configuration
    (
        string AuthenticationScheme,
        string? LoginPath,
        string? ConsentPath
    );
}
