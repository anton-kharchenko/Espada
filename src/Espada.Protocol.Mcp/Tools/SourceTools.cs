using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Application.UseCases.Sources.Commands.RegisterSource;
using Espada.Protocol.Mcp.Contracts.Requests;
using Espada.Protocol.Mcp.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Espada.Application.Constants;

namespace Espada.Protocol.Mcp.Tools
{
    [McpServerToolType]
    public sealed class SourceTools(McpApplicationExecutor executor)
    {
        [McpServerTool(
            Name = "source.register",
            Title = "Register source",
            ReadOnly = false,
            Destructive = false,
            Idempotent = false,
            OpenWorld = false,
            UseStructuredContent = true,
            OutputSchemaType = typeof(RegisterSourceResponse))]
        [Description("Registers a typed source in the authorized workspace.")]
        public async Task<RegisterSourceResponse> RegisterAsync(
            [Description("Workspace, source name, and typed source definition.")]
            SourceRegisterRequest request,
            CancellationToken cancellationToken)
        {
            await executor.AuthorizeWorkspaceAsync(
                request.WorkspaceId,
                ApplicationScopeConstants.SourceWrite,
                cancellationToken);
            RegisterSourceCommand command =
                executor.Map<RegisterSourceCommand>(request);

            return await executor.SendAsync(command, cancellationToken);
        }

        [McpServerTool(
            Name = "source.import",
            Title = "Import source",
            ReadOnly = false,
            Destructive = false,
            Idempotent = true,
            OpenWorld = true,
            UseStructuredContent = true,
            OutputSchemaType = typeof(RequestImportResponse))]
        [Description("Requests an idempotent import of a registered source.")]
        public async Task<RequestImportResponse> ImportAsync(
            [Description("Workspace, source, idempotency key, and import options.")]
            SourceImportRequest request,
            CancellationToken cancellationToken)
        {
            await executor.AuthorizeWorkspaceAsync(
                request.WorkspaceId,
                ApplicationScopeConstants.SourceWrite,
                cancellationToken);
            RequestImportCommand command =
                executor.Map<RequestImportCommand>(request);

            return await executor.SendAsync(command, cancellationToken);
        }
    }
}