using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Artifacts.Queries.ListArtifacts
{
    internal sealed class ListArtifactsQueryHandler(
        IWorkspaceRepository workspaceRepository,
        IArtifactRepository artifactRepository)
        : IQueryHandler<ListArtifactsQuery, ListArtifactsResponse>
    {
        public async Task<DomainResult<ListArtifactsResponse>> Handle(
            ListArtifactsQuery request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult<ListArtifactsResponse>.Failure(
                    WorkspaceApplicationErrors.InvalidId);
            }

            WorkspaceId workspaceId =
                WorkspaceId.Create(request.WorkspaceId);

            Workspace? workspace = await workspaceRepository.GetByIdAsync(
                workspaceId,
                cancellationToken);

            if (workspace is null)
            {
                return DomainResult<ListArtifactsResponse>.Failure(
                    WorkspaceApplicationErrors.NotFound(request.WorkspaceId));
            }

            IReadOnlyList<Artifact> artifacts =
                await artifactRepository.ListByWorkspaceIdAsync(
                    workspace.Id,
                    cancellationToken);

            ArtifactListItemResponse[] items = artifacts
                .OrderByDescending(artifact => artifact.UpdatedAtUtc)
                .Select(artifact => new ArtifactListItemResponse(
                    artifact.Id.Value,
                    artifact.Title.Value,
                    artifact.Type.Id,
                    artifact.Type.Name,
                    artifact.Status.Id,
                    artifact.Status.Name,
                    artifact.CurrentRevisionId?.Value,
                    artifact.CurrentRevisionNumber?.Value,
                    artifact.RevisionCount,
                    artifact.CreatedAtUtc,
                    artifact.UpdatedAtUtc,
                    artifact.ArchivedAtUtc))
                .ToArray();

            return DomainResult<ListArtifactsResponse>.Success(
                new ListArtifactsResponse(items));
        }
    }
}