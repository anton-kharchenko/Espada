using Espada.Application.Constants;
using Espada.Application.UseCases.Artifacts.Queries.GetArtifactById;
using Espada.Application.UseCases.Artifacts.Queries.GetArtifactRevisionById;
using Espada.Application.UseCases.Artifacts.Queries.ListArtifactRevisions;
using Espada.Application.UseCases.Chunks.Queries.GetChunkById;
using Espada.Protocol.Mcp.Services;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Espada.Protocol.Mcp.Resources
{
    [McpServerResourceType]
    public sealed class ArtifactResources(McpApplicationExecutor executor)
    {
        [McpServerResource(
            UriTemplate = "artifact://{id}",
            Name = "artifact")]
        [Description("Returns a canonical artifact and its current revision.")]
        public async Task<ResourceContents> GetArtifactAsync(
            string id,
            CancellationToken cancellationToken)
        {
            Guid artifactId = McpResourceSerializer.ParseId(id, nameof(id));
            Guid workspaceId = GetPrincipalWorkspaceId();
            await executor.AuthorizeWorkspaceAsync(
                workspaceId,
                ApplicationScopeConstants.ArtifactRead,
                cancellationToken);
            GetArtifactByIdResponse response = await executor.SendAsync(
                new GetArtifactByIdQuery(workspaceId, artifactId),
                cancellationToken);

            return McpResourceSerializer.Create(
                $"artifact://{artifactId:D}",
                new McpResourceProvenance(
                    workspaceId,
                    artifactId,
                    response.CurrentRevisionId,
                    response.CurrentRevisionNumber),
                response);
        }

        [McpServerResource(
            UriTemplate = "artifact://{id}/revision/{number}",
            Name = "artifact-revision")]
        [Description("Returns an immutable artifact revision by revision number.")]
        public async Task<ResourceContents> GetArtifactRevisionAsync(
            string id,
            int number,
            CancellationToken cancellationToken)
        {
            Guid artifactId = McpResourceSerializer.ParseId(id, nameof(id));
            if (number <= 0)
            {
                throw McpErrorMapper.InvalidArgument(
                    "Revision number must be positive.");
            }

            Guid workspaceId = GetPrincipalWorkspaceId();
            await executor.AuthorizeWorkspaceAsync(
                workspaceId,
                ApplicationScopeConstants.ArtifactRead,
                cancellationToken);
            ListArtifactRevisionsResponse revisions = await executor.SendAsync(
                new ListArtifactRevisionsQuery(workspaceId, artifactId),
                cancellationToken);
            ArtifactRevisionListItemResponse? revision = revisions.Items
                .SingleOrDefault(item => item.Number == number);
            if (revision is null)
            {
                throw McpErrorMapper.NotFound(
                    $"Artifact '{artifactId:D}' does not contain revision {number}.");
            }

            GetArtifactRevisionByIdResponse response = await executor.SendAsync(
                new GetArtifactRevisionByIdQuery(
                    workspaceId,
                    artifactId,
                    revision.Id),
                cancellationToken);

            return McpResourceSerializer.Create(
                $"artifact://{artifactId:D}/revision/{number}",
                new McpResourceProvenance(
                    workspaceId,
                    artifactId,
                    response.Id,
                    response.Number),
                response);
        }

        [McpServerResource(
            UriTemplate = "chunk://{id}",
            Name = "chunk")]
        [Description("Returns canonical chunk content without its embedding vector.")]
        public async Task<ResourceContents> GetChunkAsync(
            string id,
            CancellationToken cancellationToken)
        {
            Guid chunkId = McpResourceSerializer.ParseId(id, nameof(id));
            Guid workspaceId = GetPrincipalWorkspaceId();
            await executor.AuthorizeWorkspaceAsync(
                workspaceId,
                ApplicationScopeConstants.ArtifactRead,
                cancellationToken);
            GetChunkByIdResponse response = await executor.SendAsync(
                new GetChunkByIdQuery(workspaceId, chunkId),
                cancellationToken);

            return McpResourceSerializer.Create(
                $"chunk://{chunkId:D}",
                new McpResourceProvenance(
                    workspaceId,
                    response.ArtifactId,
                    response.ArtifactRevisionId,
                    ChunkId: chunkId),
                response);
        }

        private Guid GetPrincipalWorkspaceId()
        {
            return executor.Principal.WorkspaceId
                   ?? throw McpErrorMapper.Unauthorized(
                       "The MCP principal is not bound to a workspace.");
        }
    }
}