using System.Text.Json.Serialization;

namespace Espada.Mcp.Responses
{
    public sealed record BootstrapLinkResponse(
        [property: JsonPropertyName("url")] string Url,
        [property: JsonPropertyName("expires_in")]
        int ExpiresInSeconds);
}