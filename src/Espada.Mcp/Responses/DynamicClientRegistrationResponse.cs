using System.Text.Json.Serialization;

namespace Espada.Mcp.Responses
{
    internal sealed record DynamicClientRegistrationResponse(
        [property: JsonPropertyName("client_id")]
        string ClientId,
        [property: JsonPropertyName("client_id_issued_at")]
        long ClientIdIssuedAt,
        [property: JsonPropertyName("client_name")]
        string ClientName,
        [property: JsonPropertyName("redirect_uris")]
        IReadOnlyList<string> RedirectUris,
        [property: JsonPropertyName("token_endpoint_auth_method")]
        string TokenEndpointAuthMethod,
        [property: JsonPropertyName("grant_types")]
        IReadOnlyList<string> GrantTypes,
        [property: JsonPropertyName("response_types")]
        IReadOnlyList<string> ResponseTypes,
        [property: JsonPropertyName("scope")] string Scope);
}