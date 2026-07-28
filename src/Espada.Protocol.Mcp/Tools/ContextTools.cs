using Espada.AgentAdapters.Context;
using Espada.Application.UseCases.Context.Queries.BuildContext;
using Espada.Protocol.Mcp.Contracts.Requests;
using Espada.Protocol.Mcp.Contracts.Responses;
using Espada.Protocol.Mcp.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Espada.Application.Constants;

namespace Espada.Protocol.Mcp.Tools
{
    [McpServerToolType]
    public sealed class ContextTools(McpApplicationExecutor executor)
    {
        [McpServerTool(Name = "context.build", Title = "Build context", ReadOnly = true, Destructive = false,
            Idempotent = true, OpenWorld = false, UseStructuredContent = true,
            OutputSchemaType = typeof(ContextBuildToolResponse))]
        [Description("Resolves canonical context and renders the selected agent projection.")]
        public async Task<ContextBuildToolResponse> BuildAsync(
            [Description("Workspace selectors, agent, and UTF-8 byte budget.")] ContextBuildRequest request,
            CancellationToken cancellationToken)
        {
            await executor.AuthorizeWorkspaceAsync(request.WorkspaceId, ApplicationScopeConstants.ContextRead,
                cancellationToken);
            BuildContextQuery query = executor.Map<BuildContextQuery>(request);
            BuildContextResponse context = await executor.SendAsync(query, cancellationToken);
            AgentContextProjection projection = AgentContextProjectionRenderer.Render(context);

            return new ContextBuildToolResponse(context, projection);
        }
    }
}