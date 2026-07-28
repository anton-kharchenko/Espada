using Espada.Application.UseCases.Artifacts.Commands.AddArtifactRevision;
using Espada.Application.UseCases.Artifacts.Commands.CreateArtifact;
using Espada.Application.UseCases.Artifacts.Queries.GetArtifactById;
using Espada.Application.UseCases.Artifacts.Queries.ListArtifacts;
using Espada.Protocol.Mcp.Contracts.Requests;
using Espada.Protocol.Mcp.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Espada.Application.Constants;

namespace Espada.Protocol.Mcp.Tools
{
    [McpServerToolType]
    public sealed class ArtifactTools(McpApplicationExecutor executor)
    {
        [McpServerTool(Name = "artifact.create", Title = "Create artifact", ReadOnly = false, Destructive = false,
            Idempotent = false, OpenWorld = false, UseStructuredContent = true,
            OutputSchemaType = typeof(CreateArtifactResponse))]
        [Description("Creates a canonical artifact and its first revision.")]
        public async Task<CreateArtifactResponse> CreateAsync(
            [Description("Artifact header, content, and optional typed rules.")]
            ArtifactCreateRequest request,
            CancellationToken cancellationToken)
        {
            await executor.AuthorizeWorkspaceAsync(request.WorkspaceId, ApplicationScopeConstants.ArtifactWrite,
                cancellationToken);
            CreateArtifactCommand command = executor.Map<CreateArtifactCommand>(request);

            return await executor.SendAsync(command, cancellationToken);
        }

        [McpServerTool(Name = "artifact.revise", Title = "Revise artifact", ReadOnly = false, Destructive = false,
            Idempotent = false, OpenWorld = false, UseStructuredContent = true,
            OutputSchemaType = typeof(AddArtifactRevisionResponse))]
        [Description("Adds an immutable revision to a canonical artifact.")]
        public async Task<AddArtifactRevisionResponse> ReviseAsync(
            [Description("Artifact identifier, content, and optional typed rules.")]
            ArtifactReviseRequest request,
            CancellationToken cancellationToken)
        {
            await executor.AuthorizeWorkspaceAsync(request.WorkspaceId, ApplicationScopeConstants.ArtifactWrite,
                cancellationToken);
            AddArtifactRevisionCommand command = executor.Map<AddArtifactRevisionCommand>(request);

            return await executor.SendAsync(command, cancellationToken);
        }

        [McpServerTool(Name = "artifact.get", Title = "Get artifact", ReadOnly = true, Destructive = false,
            Idempotent = true, OpenWorld = false, UseStructuredContent = true,
            OutputSchemaType = typeof(GetArtifactByIdResponse))]
        [Description("Returns a canonical artifact and its current revision.")]
        public async Task<GetArtifactByIdResponse> GetAsync(
            [Description("Workspace and artifact identifiers.")]
            ArtifactGetRequest request,
            CancellationToken cancellationToken)
        {
            await executor.AuthorizeWorkspaceAsync(request.WorkspaceId, ApplicationScopeConstants.ArtifactRead,
                cancellationToken);
            GetArtifactByIdQuery query = executor.Map<GetArtifactByIdQuery>(request);

            return await executor.SendAsync(query, cancellationToken);
        }

        [McpServerTool(Name = "artifact.list", Title = "List artifacts", ReadOnly = true, Destructive = false,
            Idempotent = true, OpenWorld = false, UseStructuredContent = true,
            OutputSchemaType = typeof(ListArtifactsResponse))]
        [Description("Lists canonical artifacts in the authorized workspace.")]
        public async Task<ListArtifactsResponse> ListAsync(
            [Description("Authorized workspace identifier.")]
            ArtifactListRequest request,
            CancellationToken cancellationToken)
        {
            await executor.AuthorizeWorkspaceAsync(request.WorkspaceId, ApplicationScopeConstants.ArtifactRead,
                cancellationToken);
            ListArtifactsQuery query = executor.Map<ListArtifactsQuery>(request);

            return await executor.SendAsync(query, cancellationToken);
        }
    }
}