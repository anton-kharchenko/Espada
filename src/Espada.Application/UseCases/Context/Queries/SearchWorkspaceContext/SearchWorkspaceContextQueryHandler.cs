using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Context.Queries.SearchWorkspaceContext;

internal sealed class SearchWorkspaceContextQueryHandler(
    IWorkspaceRepository workspaceRepository,
    IChunkEmbeddingRepository chunkEmbeddingRepository,
    IWorkspaceContextSearchStore searchStore,
    IClockService clockService,
    IMapper mapper) : IQueryHandler<SearchWorkspaceContextQuery, SearchWorkspaceContextResponse>
{
    public async Task<DomainResult<SearchWorkspaceContextResponse>> Handle(SearchWorkspaceContextQuery request, CancellationToken cancellationToken)
    {
        if (request.WorkspaceId == Guid.Empty)
        {
            return DomainResult<SearchWorkspaceContextResponse>.Failure(WorkspaceApplicationErrors.InvalidId);
        }

        Workspace? workspace = await workspaceRepository.GetByIdAsync(WorkspaceId.Create(request.WorkspaceId), cancellationToken);
        if (workspace is null)
        {
            return DomainResult<SearchWorkspaceContextResponse>.Failure(WorkspaceApplicationErrors.NotFound(request.WorkspaceId));
        }

        DomainResult<EmbeddingModel> modelResult = EmbeddingModel.Create(request.ModelIdentifier, request.ModelVersion);
        if (modelResult.IsFailure)
        {
            return DomainResult<SearchWorkspaceContextResponse>.Failure(modelResult.Error);
        }

        IReadOnlyList<int> storedDimensions = await chunkEmbeddingRepository.ListDimensionsAsync(workspace.Id, modelResult.Value, cancellationToken);
        switch (storedDimensions.Count)
        {
            case 0:
                return DomainResult<SearchWorkspaceContextResponse>.Success(new SearchWorkspaceContextResponse([]));
            case > 1:
                return DomainResult<SearchWorkspaceContextResponse>.Failure(ChunkEmbeddingApplicationErrors.InconsistentModelDimensions(modelResult.Value.Identifier, modelResult.Value.Version));
        }

        if (storedDimensions[0] != request.QueryVector.Count)
        {
            return DomainResult<SearchWorkspaceContextResponse>.Failure(ChunkEmbeddingApplicationErrors.DimensionMismatch(modelResult.Value.Identifier, modelResult.Value.Version, storedDimensions[0], request.QueryVector.Count));
        }

        WorkspaceContextSearch search = mapper.Map<WorkspaceContextSearch>(new WorkspaceContextSearchMappingSource(request, modelResult.Value, clockService.UtcNow));
        IReadOnlyList<WorkspaceContextSearchHit> hits = await searchStore.SearchAsync(search, cancellationToken);
        WorkspaceContextItemResponse[] items = mapper.Map<WorkspaceContextItemResponse[]>(hits);

        return DomainResult<SearchWorkspaceContextResponse>.Success(new SearchWorkspaceContextResponse(items));
    }
}