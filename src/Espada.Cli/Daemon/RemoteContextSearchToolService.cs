using Espada.Protocol.Mcp.Contracts.Requests;
using Espada.Protocol.Mcp.Contracts.Responses;
using Espada.Protocol.Mcp.Service;
using System.Net.Http.Json;

namespace Espada.Cli.Daemon;

internal sealed class RemoteContextSearchToolService(HttpClient httpClient) : IContextSearchToolService
{
    public async Task<ContextSearchResponse> SearchAsync(ContextSearchRequest request, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("internal/context/search", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ContextSearchResponse>(cancellationToken) ?? throw new InvalidOperationException("Espada daemon returned an empty context search response.");
    }
}