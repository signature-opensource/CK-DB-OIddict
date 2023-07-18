using CK.Core;

namespace CK.DB.OIddict.Commands
{
    public interface ISimpleCrisResult : IPoco
    {
        bool Success { get; set; }
        string? ErrorMessage { get; set; }
    }

    internal  static class PocoDirectoryExtensions
    {
        public static ISimpleCrisResult Success(this PocoDirectory pocoDirectory)
        {
            return pocoDirectory.Create<ISimpleCrisResult>( crisResult => crisResult.Success = true );
        }

        public static ISimpleCrisResult Failure( this PocoDirectory pocoDirectory, string message )
        {
            return pocoDirectory.Create<ISimpleCrisResult>( crisResult => crisResult.ErrorMessage = message );
        }
    }
}
