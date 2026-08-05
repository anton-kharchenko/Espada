using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Embedding;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Memories.Queries.SearchMemory
{
    internal sealed class SearchMemoryQueryHandler(
        IWorkspaceRepository workspaceRepository,
        IMemorySearchStore memorySearchStore,
        IWorkspaceContextSearchStore workspaceContextSearchStore,
        IEmbeddingModelDefaults embeddingModelDefaults,
        IEmbeddingGeneratorService embeddingGeneratorService,
        IClockService clockService,
        IMapper mapper)
        : IQueryHandler<SearchMemoryQuery, SearchMemoryResponse>
    {
        public async Task<DomainResult<SearchMemoryResponse>> Handle(
            SearchMemoryQuery request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure<SearchMemoryResponse>(
                    WorkspaceApplicationErrors.InvalidId);
            }

            if (string.IsNullOrWhiteSpace(request.QueryText))
            {
                return DomainResult.Failure<SearchMemoryResponse>(
                    MemoryApplicationErrors.QueryEmpty);
            }

            if (request.TopK is < 1 or > 50)
            {
                return DomainResult.Failure<SearchMemoryResponse>(
                    MemoryApplicationErrors.TopKOutOfRange);
            }

            WorkspaceId workspaceId = WorkspaceId.Create(request.WorkspaceId);
            Workspace? workspace = await workspaceRepository.GetByIdAsync(
                workspaceId,
                cancellationToken);
            if (workspace is null)
            {
                return DomainResult.Failure<SearchMemoryResponse>(
                    WorkspaceApplicationErrors.NotFound(request.WorkspaceId));
            }

            List<MemoryCategoryType> categoryTypes = [];
            foreach (int categoryTypeId in request.CategoryTypeIds ?? [])
            {
                MemoryCategoryType? categoryType = Enumeration
                    .GetAll<MemoryCategoryType>()
                    .SingleOrDefault(value => value.Id == categoryTypeId);
                if (categoryType is null)
                {
                    return DomainResult.Failure<SearchMemoryResponse>(
                        MemoryApplicationErrors.UnsupportedCategoryType(categoryTypeId));
                }

                categoryTypes.Add(categoryType);
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
                    return DomainResult.Failure<SearchMemoryResponse>(
                        MemoryApplicationErrors.InvalidEmbeddingModel);
                }

                DomainResult<EmbeddingModel> modelResult = EmbeddingModel.Create(
                    parts[0],
                    parts[1]);
                if (modelResult.IsFailure)
                {
                    return DomainResult.Failure<SearchMemoryResponse>(modelResult.Error);
                }

                model = modelResult.Value;
            }

            IReadOnlyList<float> queryVector = [];
            if (model is not null)
            {
                GeneratedEmbedding generatedEmbedding = await embeddingGeneratorService.GenerateAsync(
                    model.Identifier,
                    model.Version,
                    request.QueryText.Trim(),
                    cancellationToken);
                queryVector = generatedEmbedding.Vector;
            }

            string[] categoryNames = categoryTypes
                .Select(categoryType => categoryType.Name)
                .ToArray();
            WorkspaceContextSearch search = mapper.Map<WorkspaceContextSearch>(
                new MemoryWorkspaceContextSearchMappingSource(
                    request,
                    model,
                    queryVector,
                    categoryNames,
                    clockService.UtcNow));
            IReadOnlyList<WorkspaceContextSearchHit> hits =
                await workspaceContextSearchStore.SearchAsync(search, cancellationToken);
            IReadOnlyList<MemorySearchRecord> records = await memorySearchStore.LoadAsync(
                workspaceId,
                hits,
                categoryTypes,
                cancellationToken);
            MemorySearchItemResponse[] items =
                mapper.Map<MemorySearchItemResponse[]>(records);

            return DomainResult.Success(new SearchMemoryResponse(items));
        }
    }
}