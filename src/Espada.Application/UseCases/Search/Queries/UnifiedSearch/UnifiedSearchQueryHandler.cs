using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Embedding;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Search.Queries.UnifiedSearch
{
    internal sealed class UnifiedSearchQueryHandler(
        IWorkspaceRepository workspaceRepository,
        IWorkspaceContextSearchStore searchStore,
        IUnifiedSearchMetadataStore metadataStore,
        IEmbeddingModelDefaults embeddingModelDefaults,
        IEmbeddingGeneratorService embeddingGenerator,
        IClockService clockService)
        : IQueryHandler<UnifiedSearchQuery, UnifiedSearchResponse>
    {
        public async Task<DomainResult<UnifiedSearchResponse>> Handle(UnifiedSearchQuery request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure<UnifiedSearchResponse>(WorkspaceApplicationErrors.InvalidId);
            }

            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return DomainResult.Failure<UnifiedSearchResponse>(UnifiedSearchApplicationErrors.QueryEmpty);
            }

            if (request.Limit is < 1 or > 50)
            {
                return DomainResult.Failure<UnifiedSearchResponse>(UnifiedSearchApplicationErrors.LimitOutOfRange);
            }

            WorkspaceId workspaceId = WorkspaceId.Create(request.WorkspaceId);
            Workspace? workspace = await workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
            if (workspace is null)
            {
                return DomainResult.Failure<UnifiedSearchResponse>(
                    WorkspaceApplicationErrors.NotFound(request.WorkspaceId));
            }

            EmbeddingModel? model = null;
            string? defaultModel = embeddingModelDefaults.DefaultModel;
            if (!string.IsNullOrWhiteSpace(defaultModel))
            {
                string[] parts = defaultModel.Split(
                    '@',
                    2,
                    StringSplitOptions.TrimEntries);
                if (parts.Length != 2 || parts.Any(part => part.Length == 0))
                {
                    return DomainResult.Failure<UnifiedSearchResponse>(
                        UnifiedSearchApplicationErrors.InvalidEmbeddingModel);
                }

                DomainResult<EmbeddingModel> modelResult = EmbeddingModel.Create(parts[0], parts[1]);
                if (modelResult.IsFailure)
                {
                    return DomainResult.Failure<UnifiedSearchResponse>(modelResult.Error);
                }

                model = modelResult.Value;
            }

            IReadOnlyList<float> queryVector = [];
            if (model is not null)
            {
                GeneratedEmbedding embedding = await embeddingGenerator.GenerateAsync(model.Identifier,
                    model.Version, request.Query.Trim(), cancellationToken);
                queryVector = embedding.Vector;
            }

            WorkspaceContextSearch search = new(workspaceId.Value, request.Query.Trim(), queryVector,
                model?.Identifier ?? string.Empty, model?.Version ?? string.Empty, request.Limit, [], [], [], [], [],
                [], [], null, null, null, null, clockService.UtcNow);
            IReadOnlyList<WorkspaceContextSearchHit> hits = await searchStore.SearchAsync(search, cancellationToken);
            IReadOnlyList<UnifiedSearchRecord> records = await metadataStore.LoadAsync(workspaceId, hits,
                cancellationToken);
            UnifiedSearchItemResponse[] items = records.Select(record => new UnifiedSearchItemResponse(
                record.HitType, record.ChunkId, record.ArtifactId, record.RevisionId, record.SourceId,
                record.SourceTypeId, record.ArtifactKind, record.ArtifactTypeId, record.Title, record.Content,
                record.SourceSpanStart, record.SourceSpanLength, record.Score, record.Provenance)).ToArray();
            return DomainResult.Success(new UnifiedSearchResponse(items));
        }

    }
}
