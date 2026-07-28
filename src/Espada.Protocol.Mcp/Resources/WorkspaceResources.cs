using Espada.Application.UseCases.Context.Queries.BuildContext;
using Espada.Domain.Enums;
using Espada.Protocol.Mcp.Contracts.Responses;
using Espada.Protocol.Mcp.Services;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Espada.Application.Constants;

namespace Espada.Protocol.Mcp.Resources
{
    [McpServerResourceType]
    public sealed class WorkspaceResources(McpApplicationExecutor executor)
    {
        [McpServerResource(UriTemplate = "workspace://{id}/instructions", Name = "workspace-instructions")]
        [Description("Returns canonical workspace-level instruction context.")]
        public async Task<ResourceContents> GetInstructionsAsync(string id, CancellationToken cancellationToken)
        {
            Guid workspaceId = McpResourceSerializer.ParseId(id, nameof(id));
            return await GetContextResourceAsync(
                workspaceId,
                ArtifactKindType.Instruction.Name,
                ApplicationScopeConstants.ArtifactRead,
                $"workspace://{workspaceId:D}/instructions",
                cancellationToken);
        }

        [McpServerResource(UriTemplate = "workspace://{id}/memory", Name = "workspace-memory")]
        [Description("Returns canonical workspace-level shared memory.")]
        public async Task<ResourceContents> GetMemoryAsync(
            string id,
            CancellationToken cancellationToken)
        {
            Guid workspaceId = McpResourceSerializer.ParseId(id, nameof(id));
            return await GetContextResourceAsync(
                workspaceId,
                ArtifactKindType.Memory.Name,
                ApplicationScopeConstants.MemoryRead,
                $"workspace://{workspaceId:D}/memory",
                cancellationToken);
        }

        private async Task<ResourceContents> GetContextResourceAsync(
            Guid workspaceId,
            string artifactKind,
            string requiredScope,
            string uri,
            CancellationToken cancellationToken)
        {
            await executor.AuthorizeWorkspaceAsync(
                workspaceId,
                requiredScope,
                cancellationToken);
            BuildContextQuery query = new(
                workspaceId,
                null,
                null,
                null,
                null,
                ContextAgentConstants.Generic,
                int.MaxValue);
            BuildContextResponse context = await executor.SendAsync(
                query,
                cancellationToken);
            ContextItemResponse[] items = context.IncludedItems
                .Where(item => item.ArtifactKind.Equals(
                    artifactKind,
                    StringComparison.OrdinalIgnoreCase))
                .ToArray();
            WorkspaceContextResourceResponse response = new(
                workspaceId,
                artifactKind,
                items);

            return McpResourceSerializer.Create(
                uri,
                new McpResourceProvenance(workspaceId),
                response);
        }
    }
}