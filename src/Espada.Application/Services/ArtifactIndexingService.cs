using Espada.Application.Contracts.Billing;
using Espada.Application.Contracts.Billing.Constants;
using Espada.Application.Contracts.Embedding;
using Espada.Application.Contracts.Ingestion;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.Enums;
using Espada.Application.Exceptions;
using Espada.Application.Models;
using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using System.Security.Cryptography;
using System.Text;

namespace Espada.Application.Services
{
    internal sealed class ArtifactIndexingService(
        IChunkBatchRepository chunkBatchRepository,
        IChunkRepository chunkRepository,
        IChunkEmbeddingRepository chunkEmbeddingRepository,
        IEmbeddingVectorStore embeddingVectorStore,
        IEnumerable<IChunkingStrategy> chunkingStrategies,
        IBatchEmbeddingGeneratorService embeddingGenerator,
        IClockService clock,
        IUsageMeterService usageMeterService)
    {
        private const int EmbeddingBatchSize = 64;
        private readonly IReadOnlyDictionary<string, IChunkingStrategy> _chunkingStrategies =
            chunkingStrategies.ToDictionary(
                strategy => strategy.Name,
                StringComparer.OrdinalIgnoreCase);

        public async Task<ArtifactChunkingResult> ChunkAsync(
            Guid operationId,
            WorkspaceId workspaceId,
            ArtifactId artifactId,
            ArtifactRevisionId revisionId,
            string content,
            ImportOptions options,
            CancellationToken cancellationToken)
        {
            if (!_chunkingStrategies.TryGetValue(options.ChunkingStrategy, out IChunkingStrategy? strategy))
            {
                throw Permanent(
                    IngestionFailureCodes.UnsupportedChunkingStrategy,
                    $"Chunking strategy '{options.ChunkingStrategy}' is not registered.");
            }

            IReadOnlyList<ChunkSegment> segments = await strategy.ChunkAsync(
                content,
                options,
                cancellationToken);
            if (segments.Count == 0)
            {
                throw Permanent(
                    IngestionFailureCodes.EmptyChunkBatch,
                    "Chunking produced no chunks.");
            }

            ChunkingStrategyType strategyType = Enumeration
                .GetAll<ChunkingStrategyType>()
                .Single(value => value.Name.Equals(strategy.Name, StringComparison.OrdinalIgnoreCase));
            ChunkBatchId batchId = ChunkBatchId.Create(
                DeterministicGuid(operationId, ImportPipelineDiscriminators.ChunkBatch));
            DomainResult<ChunkingVersion> version = ChunkingVersion.Create(
                ImportPipelineDiscriminators.ChunkingVersion);
            EnsureSuccess(version);
            DomainResult<ChunkBatch> batchResult = ChunkBatch.Request(
                batchId,
                workspaceId,
                artifactId,
                revisionId,
                strategyType,
                version.Value,
                clock.UtcNow);
            EnsureSuccess(batchResult);
            ChunkBatch batch = batchResult.Value;
            EnsureSuccess(batch.Start(clock.UtcNow));

            List<Chunk> chunks = new(segments.Count);
            foreach (ChunkSegment segment in segments)
            {
                DomainResult<ChunkNumber> number = ChunkNumber.Create(segment.Number);
                DomainResult<ChunkContent> chunkContent = ChunkContent.Create(segment.Content);
                DomainResult<SourceTextSpan> span = SourceTextSpan.Create(
                    segment.Start,
                    segment.Length);
                EnsureSuccess(number);
                EnsureSuccess(chunkContent);
                EnsureSuccess(span);
                DomainResult<Chunk> chunk = Chunk.Create(
                    ChunkId.Create(DeterministicGuid(operationId, $"chunk:{segment.Number}")),
                    batch.Id,
                    workspaceId,
                    artifactId,
                    revisionId,
                    number.Value,
                    chunkContent.Value,
                    span.Value,
                    strategyType,
                    version.Value,
                    clock.UtcNow);
                EnsureSuccess(chunk);
                chunks.Add(chunk.Value);
            }

            EnsureSuccess(batch.Complete(chunks.Count, clock.UtcNow));
            await chunkBatchRepository.AddAsync(batch, cancellationToken);
            await chunkRepository.AddRangeAsync(chunks, cancellationToken);

            return new ArtifactChunkingResult(batch, chunks);
        }

        public async Task EmbedAndIndexAsync(
            Guid operationId,
            WorkspaceId workspaceId,
            ArtifactRevisionId revisionId,
            string embeddingModel,
            string usageKey,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<Chunk> chunks = await chunkRepository.ListByArtifactRevisionIdAsync(
                revisionId,
                cancellationToken);
            await EmbedAndIndexAsync(
                operationId,
                workspaceId,
                chunks,
                embeddingModel,
                usageKey,
                cancellationToken);
        }

        private async Task EmbedAndIndexAsync(
            Guid operationId,
            WorkspaceId workspaceId,
            IReadOnlyList<Chunk> chunks,
            string embeddingModel,
            string usageKey,
            CancellationToken cancellationToken)
        {
            (string identifier, string version) = ParseModel(embeddingModel);
            DomainResult<EmbeddingModel> model = EmbeddingModel.Create(identifier, version);
            EnsureSuccess(model);
            IReadOnlyList<int> storedDimensions = await chunkEmbeddingRepository.ListDimensionsAsync(
                workspaceId,
                model.Value,
                cancellationToken);
            if (storedDimensions.Count > 1)
            {
                throw Permanent(
                    IngestionFailureCodes.EmbeddingDimensionMismatch,
                    "Stored vectors for the configured model have inconsistent dimensions.");
            }

            int? expectedDimensions = storedDimensions.SingleOrDefault();
            long inputUnits = 0;

            for (int offset = 0; offset < chunks.Count; offset += EmbeddingBatchSize)
            {
                Chunk[] batch = chunks.Skip(offset).Take(EmbeddingBatchSize).ToArray();
                IReadOnlyList<GeneratedEmbedding> generated = await embeddingGenerator.GenerateBatchAsync(
                    identifier,
                    version,
                    batch.Select(chunk => chunk.Content.Value).ToArray(),
                    cancellationToken);
                inputUnits += generated.Sum(item => item.InputUnits);
                if (generated.Count != batch.Length)
                {
                    throw new IngestionException(
                        JobFailureCategoryType.Transient,
                        IngestionFailureCodes.EmbeddingCountMismatch,
                        "Embedding provider returned a different number of vectors than inputs.");
                }

                int? dimensions = null;
                for (int index = 0; index < batch.Length; index++)
                {
                    Chunk chunk = batch[index];
                    IReadOnlyList<float> vector = generated[index].Vector;
                    if (vector.Count == 0 || vector.Any(value => !float.IsFinite(value)))
                    {
                        throw Permanent(
                            IngestionFailureCodes.InvalidEmbeddingVector,
                            "Embedding vector is empty or non-finite.");
                    }

                    dimensions ??= vector.Count;
                    if (dimensions != vector.Count ||
                        expectedDimensions is > 0 && expectedDimensions != vector.Count)
                    {
                        throw Permanent(
                            IngestionFailureCodes.EmbeddingDimensionMismatch,
                            "Embedding provider returned inconsistent dimensions.");
                    }

                    DomainResult<EmbeddingDimensions> embeddingDimensions =
                        EmbeddingDimensions.Create(vector.Count);
                    EnsureSuccess(embeddingDimensions);
                    ChunkEmbeddingId embeddingId = ChunkEmbeddingId.Create(
                        DeterministicGuid(
                            operationId,
                            $"embedding:{chunk.Number.Value}:{identifier}:{version}"));
                    DomainResult<ChunkEmbedding> embedding = ChunkEmbedding.Create(
                        embeddingId,
                        workspaceId,
                        chunk.Id,
                        chunk.ContentHash,
                        model.Value,
                        embeddingDimensions.Value,
                        clock.UtcNow);
                    EnsureSuccess(embedding);
                    await chunkEmbeddingRepository.AddAsync(embedding.Value, cancellationToken);
                    await embeddingVectorStore.UpsertAsync(
                        embeddingId,
                        vector,
                        cancellationToken);
                }
            }

            await usageMeterService.RecordAsync(
                workspaceId.Value,
                UsageMetricConstants.EmbeddingInputUnits,
                inputUnits,
                usageKey,
                cancellationToken);
        }

        public async Task IndexAsync(
            Guid operationId,
            WorkspaceId workspaceId,
            ArtifactId artifactId,
            ArtifactRevisionId revisionId,
            string content,
            ImportOptions options,
            string usageKey,
            CancellationToken cancellationToken)
        {
            ArtifactChunkingResult chunkingResult = await ChunkAsync(
                operationId,
                workspaceId,
                artifactId,
                revisionId,
                content,
                options,
                cancellationToken);
            if (!string.IsNullOrWhiteSpace(options.EmbeddingModel))
            {
                await EmbedAndIndexAsync(
                    operationId,
                    workspaceId,
                    chunkingResult.Chunks,
                    options.EmbeddingModel,
                    usageKey,
                    cancellationToken);
            }
        }

        private static (string Identifier, string Version) ParseModel(string? model)
        {
            string[] parts = model?.Split('@', 2, StringSplitOptions.TrimEntries) ?? [];
            return parts.Length == 2 && parts.All(part => part.Length > 0)
                ? (parts[0], parts[1])
                : throw Permanent(
                    IngestionFailureCodes.InvalidEmbeddingModel,
                    "Embedding model must use 'identifier@version' format.");
        }

        private static Guid DeterministicGuid(Guid operationId, string discriminator)
        {
            byte[] hash = SHA256.HashData(
                Encoding.UTF8.GetBytes($"{operationId:N}:{discriminator}"));
            return new Guid(hash.AsSpan(0, 16));
        }

        private static void EnsureSuccess(DomainResult result)
        {
            if (result.IsFailure)
            {
                throw Permanent(result.Error.Code, result.Error.Description);
            }
        }

        private static void EnsureSuccess<T>(DomainResult<T> result)
        {
            if (result.IsFailure)
            {
                throw Permanent(result.Error.Code, result.Error.Description);
            }
        }

        private static IngestionException Permanent(string code, string message)
        {
            return new IngestionException(JobFailureCategoryType.Permanent, code, message);
        }
    }
}