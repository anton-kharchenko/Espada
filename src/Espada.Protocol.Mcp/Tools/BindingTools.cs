using Espada.Application.Constants;
using Espada.Application.UseCases.Bindings.Commands.RemoveBinding;
using Espada.Application.UseCases.Bindings.Commands.SetBinding;
using Espada.Application.UseCases.Bindings.Common;
using Espada.Protocol.Mcp.Contracts.Requests;
using Espada.Protocol.Mcp.Contracts.Responses;
using Espada.Protocol.Mcp.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Espada.Protocol.Mcp.Tools
{
    [McpServerToolType]
    public sealed class BindingTools(McpApplicationExecutor executor)
    {
        [McpServerTool(Name = "binding.set", Title = "Set binding", ReadOnly = false, Destructive = false,
            Idempotent = false, OpenWorld = false, UseStructuredContent = true,
            OutputSchemaType = typeof(BindingResponse))]
        [Description("Creates or replaces artifact scope selectors.")]
        public async Task<BindingResponse> SetAsync(
            [Description("Artifact binding and optional scope selectors.")]
            BindingSetRequest request,
            CancellationToken cancellationToken)
        {
            await executor.AuthorizeWorkspaceAsync(request.WorkspaceId, ApplicationScopeConstants.WorkspaceWrite,
                cancellationToken);
            SetBindingCommand command = executor.Map<SetBindingCommand>(request);

            return await executor.SendAsync(command, cancellationToken);
        }

        [McpServerTool(Name = "binding.remove", Title = "Remove binding", ReadOnly = false, Destructive = true,
            Idempotent = false, OpenWorld = false, UseStructuredContent = true,
            OutputSchemaType = typeof(McpOperationResponse))]
        [Description("Removes an artifact binding from the authorized workspace.")]
        public async Task<McpOperationResponse> RemoveAsync(
            [Description("Workspace and binding identifiers.")]
            BindingRemoveRequest request,
            CancellationToken cancellationToken)
        {
            await executor.AuthorizeWorkspaceAsync(request.WorkspaceId, ApplicationScopeConstants.WorkspaceWrite,
                cancellationToken);
            RemoveBindingCommand command = executor.Map<RemoveBindingCommand>(request);
            await executor.SendAsync(command, cancellationToken);

            return new McpOperationResponse(true);
        }
    }
}