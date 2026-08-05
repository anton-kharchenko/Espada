namespace Espada.Mcp.Security
{
    internal sealed class InvalidClientMetadataException(string message)
        : Exception(message);
}