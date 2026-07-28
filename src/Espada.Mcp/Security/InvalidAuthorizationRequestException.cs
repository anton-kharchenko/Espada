namespace Espada.Mcp.Security
{
    internal sealed class InvalidAuthorizationRequestException(
        string error,
        string message) : Exception(message)
    {
        public string Error { get; } = error;
    }
}