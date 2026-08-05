using System.Text.Json.Serialization;

namespace Espada.Mcp.Responses
{
    internal sealed record OAuthErrorResponse(
        [property: JsonPropertyName("error")] string Error,
        [property: JsonPropertyName("error_description")]
        string ErrorDescription);
}