using System.Text.Json.Serialization;

namespace Espada.Mcp.Responses
{
    internal sealed record ProtectedResourceMetadataResponse(
        [property: JsonPropertyName("resource")]
        string Resource,
        [property: JsonPropertyName("resource_name")]
        string ResourceName,
        [property: JsonPropertyName("authorization_servers")]
        IReadOnlyList<string> AuthorizationServers,
        [property: JsonPropertyName("scopes_supported")]
        IReadOnlyList<string> ScopesSupported,
        [property: JsonPropertyName("bearer_methods_supported")]
        IReadOnlyList<string> BearerMethodsSupported);
}