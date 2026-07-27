using Espada.Protocol.Mcp.Contracts.Requests;
using Espada.Protocol.Mcp.Contracts.Responses;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Espada.Protocol.Mcp.Service;

[McpServerToolType]
public sealed class ContextSearchTool(IContextSearchToolService service)
{
    [McpServerTool(Name = "context.search", Title = "Search workspace context", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false, UseStructuredContent = true, OutputSchemaType = typeof(ContextSearchResponse))]
    [Description("Returns semantically and lexically relevant chunks from an Espada workspace.")]
    public Task<ContextSearchResponse> SearchAsync([Description("Workspace, query, embedding model, result limit, and optional filters.")] ContextSearchRequest request, CancellationToken cancellationToken) =>
        service.SearchAsync(request, cancellationToken);
}