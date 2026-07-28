using Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;
using Espada.Application.UseCases.Workspaces.Common;
using Espada.Application.UseCases.Workspaces.Queries.GetWorkspaceById;
using Espada.Protocol.Mcp.Contracts.Requests;
using Espada.Protocol.Mcp.Mappings;
using Espada.Protocol.Mcp.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Espada.Application.Constants;

namespace Espada.Protocol.Mcp.Tools
{
    [McpServerToolType]
    public sealed class WorkspaceTools(McpApplicationExecutor executor)
    {
        [McpServerTool(Name = "workspace.create", Title = "Create workspace", ReadOnly = false, Destructive = false,
            Idempotent = false, OpenWorld = false, UseStructuredContent = true,
            OutputSchemaType = typeof(CreateWorkspaceResponse))]
        [Description("Creates a workspace through the bootstrap authorization grant.")]
        public async Task<CreateWorkspaceResponse> CreateAsync(
            [Description("Workspace name, type, and optional organization.")] WorkspaceCreateRequest request,
            CancellationToken cancellationToken)
        {
            executor.AuthorizeWorkspaceCreation();
            CreateWorkspaceCommand command =
                executor.Map<CreateWorkspaceCommand>(new WorkspaceCreateMappingSource(request, executor.Principal));

            return await executor.SendAsync(command, cancellationToken);
        }

        [McpServerTool(Name = "workspace.get", Title = "Get workspace", ReadOnly = true, Destructive = false,
            Idempotent = true, OpenWorld = false, UseStructuredContent = true,
            OutputSchemaType = typeof(WorkspaceResponse))]
        [Description("Returns the authorized Espada workspace.")]
        public async Task<WorkspaceResponse> GetAsync(
            [Description("Authorized workspace identifier.")] WorkspaceGetRequest request,
            CancellationToken cancellationToken)
        {
            await executor.AuthorizeWorkspaceAsync(request.WorkspaceId, ApplicationScopeConstants.WorkspaceRead,
                cancellationToken);
            GetWorkspaceByIdQuery query = executor.Map<GetWorkspaceByIdQuery>(request);

            return await executor.SendAsync(query, cancellationToken);
        }
    }
}