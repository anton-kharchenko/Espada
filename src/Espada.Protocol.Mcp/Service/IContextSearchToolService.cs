using Espada.Protocol.Mcp.Contracts.Requests;
using Espada.Protocol.Mcp.Contracts.Responses;

namespace Espada.Protocol.Mcp.Service;

public interface IContextSearchToolService
{
    Task<ContextSearchResponse> SearchAsync(ContextSearchRequest request, CancellationToken cancellationToken = default);
}