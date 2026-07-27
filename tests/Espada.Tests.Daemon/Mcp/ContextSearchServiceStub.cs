using Espada.Protocol.Mcp.Contracts.Requests;
using Espada.Protocol.Mcp.Contracts.Responses;
using Espada.Protocol.Mcp.Service;

namespace Espada.Tests.Daemon.Mcp;

internal sealed class ContextSearchServiceStub : IContextSearchToolService
{
    public ContextSearchRequest? ReceivedRequest { get; private set; }

    public Task<ContextSearchResponse> SearchAsync(ContextSearchRequest request, CancellationToken cancellationToken = default)
    {
        ReceivedRequest = request;
        return Task.FromResult(new ContextSearchResponse([]));
    }
}