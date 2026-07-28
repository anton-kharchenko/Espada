using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Artifacts.Queries.ListArtifacts
{
    internal sealed class ListArtifactsQueryHandler(
        IWorkspaceRepository workspaceRepository,
        IArtifactRepository artifactRepository,
        IMapper mapper)
        : IQueryHandler<ListArtifactsQuery, ListArtifactsResponse>
    {
        public async Task<DomainResult<ListArtifactsResponse>> Handle(
            ListArtifactsQuery request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure<ListArtifactsResponse>(
                    WorkspaceApplicationErrors.InvalidId);
            }

            ArtifactKindType? kindType = null;
            if (request.KindTypeId.HasValue)
            {
                kindType = Enumeration
                    .GetAll<ArtifactKindType>()
                    .SingleOrDefault(value =>
                        value.Id == request.KindTypeId.Value);
                if (kindType is null)
                {
                    return DomainResult.Failure<ListArtifactsResponse>(
                        ArtifactApplicationErrors.UnsupportedKindType(
                            request.KindTypeId.Value));
                }
            }

            WorkspaceId workspaceId = WorkspaceId.Create(request.WorkspaceId);
            Workspace? workspace = await workspaceRepository.GetByIdAsync(
                workspaceId,
                cancellationToken);
            if (workspace is null)
            {
                return DomainResult.Failure<ListArtifactsResponse>(
                    WorkspaceApplicationErrors.NotFound(request.WorkspaceId));
            }

            IReadOnlyList<Artifact> artifacts = await artifactRepository.ListByWorkspaceIdAsync(
                workspace.Id,
                cancellationToken);
            Artifact[] orderedArtifacts = artifacts
                .Where(artifact =>
                    kindType is null
                    || artifact.KindType.Equals(kindType))
                .OrderByDescending(artifact => artifact.UpdatedAtUtc)
                .ToArray();
            ArtifactListItemResponse[] items = mapper.Map<ArtifactListItemResponse[]>(
                orderedArtifacts);

            return DomainResult.Success(new ListArtifactsResponse(items));
        }
    }
}
