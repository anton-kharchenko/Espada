using Espada.Application.UseCases.Memories.Commands.RememberMemory;
using Espada.Application.UseCases.Memories.Queries.SearchMemory;
using Espada.Protocol.Mcp.Contracts.Requests;
using Espada.Protocol.Mcp.Mappings;
using Espada.Protocol.Mcp.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Espada.Application.Constants;

namespace Espada.Protocol.Mcp.Tools
{
    [McpServerToolType]
    public sealed class MemoryTools(McpApplicationExecutor executor)
    {
        [McpServerTool(
            Name = "memory.remember",
            Title = "Remember memory",
            ReadOnly = false,
            Destructive = false,
            Idempotent = false,
            OpenWorld = false,
            UseStructuredContent = true,
            OutputSchemaType = typeof(RememberMemoryResponse))]
        [Description("Records unconfirmed memory with client and session provenance.")]
        public async Task<RememberMemoryResponse> RememberAsync(
            [Description("Memory content, category, confidence, and provenance.")]
            MemoryRememberRequest request,
            CancellationToken cancellationToken)
        {
            await executor.AuthorizeWorkspaceAsync(
                request.WorkspaceId,
                ApplicationScopeConstants.MemoryWrite,
                cancellationToken);
            RememberMemoryCommand command = executor.Map<RememberMemoryCommand>(
                new MemoryRememberMappingSource(request, executor.Principal));

            return await executor.SendAsync(command, cancellationToken);
        }

        [McpServerTool(
            Name = "memory.search",
            Title = "Search memory",
            ReadOnly = true,
            Destructive = false,
            Idempotent = true,
            OpenWorld = false,
            UseStructuredContent = true,
            OutputSchemaType = typeof(SearchMemoryResponse))]
        [Description("Searches canonical workspace memory.")]
        public async Task<SearchMemoryResponse> SearchAsync(
            [Description("Workspace memory query and optional category filters.")]
            MemorySearchRequest request,
            CancellationToken cancellationToken)
        {
            await executor.AuthorizeWorkspaceAsync(
                request.WorkspaceId,
                ApplicationScopeConstants.MemoryRead,
                cancellationToken);
            SearchMemoryQuery query = executor.Map<SearchMemoryQuery>(request);

            return await executor.SendAsync(query, cancellationToken);
        }
    }
}