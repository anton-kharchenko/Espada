using Espada.Application.Contracts.Billing;
using Espada.Application.Contracts.Billing.Constants;
using Espada.Application.Contracts.Blobs;
using Espada.Application.Contracts.Embedding;
using Espada.Application.Contracts.Ingestion;
using Espada.Application.Contracts.Jobs;
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
using Espada.Domain.ValueObjects.SourceDefinitions;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Espada.Application.Services;

internal sealed class ImportPipelineStageExecutorService(
    IImportJobRepository importJobRepository,
    ISourceRepository sourceRepository,
    IArtifactRepository artifactRepository,
    IArtifactRevisionRepository artifactRevisionRepository,
    IChunkBatchRepository chunkBatchRepository,
    IChunkRepository chunkRepository,
    IChunkEmbeddingRepository chunkEmbeddingRepository,
    IEmbeddingVectorStore embeddingVectorStore,
    ISourceReader sourceReader,
    ISourceParser sourceParser,
    IEnumerable<IChunkingStrategy> chunkingStrategies,
    IBatchEmbeddingGeneratorService embeddingGenerator,
    IBlobStoreService blobStoreService,
    IUnitOfWork unitOfWork,
    IClockService clock,
    IUsageMeterService usageMeterService)
    : IImportPipelineStageExecutorService
{
    private const int EmbeddingBatchSize = 64;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IReadOnlyDictionary<string, IChunkingStrategy> _chunkingStrategies = chunkingStrategies.ToDictionary(strategy => strategy.Name, StringComparer.OrdinalIgnoreCase);

    public async Task ExecuteAsync(IngestionJob job, CancellationToken cancellationToken = default)
    {
        ImportJob importJob = await importJobRepository.GetByIdAsync(job.ImportJobId, cancellationToken) ?? throw Permanent(IngestionFailureCodes.ImportNotFound, "Import no longer exists.");

        if (importJob.Status.Equals(ImportStatusType.Cancelled))
        {
            throw new OperationCanceledException("Import was cancelled.", cancellationToken);
        }

        if (importJob.Status.Equals(ImportStatusType.Succeeded) || importJob.Status.Equals(ImportStatusType.Failed) || job.Stage.Id < importJob.Stage.Id)
        {
            return;
        }

        if (!job.Stage.Equals(importJob.Stage))
        {
            throw new IngestionException(JobFailureCategoryType.Poison, IngestionFailureCodes.StageMismatch, $"Job stage '{job.Stage}' does not match import stage '{importJob.Stage}'.");
        }

        if (job.Stage.Equals(ImportPipelineStageType.Start))
        {
            CompleteStage(importJob, ImportPipelineStageType.Start);
        }
        else if (job.Stage.Equals(ImportPipelineStageType.Read))
        {
            await ReadAsync(importJob, cancellationToken);
        }
        else if (job.Stage.Equals(ImportPipelineStageType.Parse))
        {
            await ParseAsync(importJob, cancellationToken);
        }
        else if (job.Stage.Equals(ImportPipelineStageType.MaterializeArtifact))
        {
            await MaterializeArtifactAsync(importJob, cancellationToken);
        }
        else if (job.Stage.Equals(ImportPipelineStageType.Chunk))
        {
            await ChunkAsync(importJob, cancellationToken);
        }
        else if (job.Stage.Equals(ImportPipelineStageType.EmbedAndIndex))
        {
            await EmbedAndIndexAsync(importJob, cancellationToken);
        }
        else if (job.Stage.Equals(ImportPipelineStageType.Complete))
        {
            await CompleteAsync(importJob, cancellationToken);
            return;
        }
        else
        {
            throw new IngestionException(JobFailureCategoryType.Poison, IngestionFailureCodes.UnknownStage, $"Unknown import stage '{job.Stage}'.");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ReadAsync(ImportJob importJob, CancellationToken cancellationToken)
    {
        Source source = await GetSourceAsync(importJob, cancellationToken);
        Stopwatch stopwatch = Stopwatch.StartNew();
        SourceReadResult read = await sourceReader.ReadAsync(source.Definition, cancellationToken);
        await using (read.Content)
        {
            BlobDescriptor raw = await blobStoreService.PutAsync(read.Content, new BlobWriteOptions(read.MediaType), cancellationToken);
            EnsureSuccess(importJob.RecordRawSnapshot(raw.Hash.Value));
            await usageMeterService.RecordAsync(importJob.WorkspaceId.Value, UsageMetricConstants.RawBytes, raw.Length, $"{importJob.Id.Value:N}:raw", cancellationToken);
        }
        stopwatch.Stop();
        await usageMeterService.RecordAsync(importJob.WorkspaceId.Value, source.Definition is ConnectorSourceDefinition ? UsageMetricConstants.PluginComputeMilliseconds : UsageMetricConstants.ParserComputeMilliseconds, Math.Max(1, stopwatch.ElapsedMilliseconds), $"{importJob.Id.Value:N}:read-ms", cancellationToken);

        CompleteStage(importJob, ImportPipelineStageType.Read);
    }

    private async Task ParseAsync(ImportJob importJob, CancellationToken cancellationToken)
    {
        Source source = await GetSourceAsync(importJob, cancellationToken);
        Stopwatch stopwatch = Stopwatch.StartNew();
        BlobHash rawHash = ParseBlobHash(importJob.RawBlobHash, ImportPipelineDiscriminators.RawBlob);
        await using Stream raw = await blobStoreService.OpenReadAsync(rawHash, cancellationToken);
        (string fileName, string mediaType) = GetSourceMetadata(source.Definition);
        string text = await sourceParser.ParseAsync(raw, fileName, mediaType, cancellationToken);
        if (string.IsNullOrWhiteSpace(text))
        {
            throw Permanent(IngestionFailureCodes.EmptyExtractedText, "Source produced no searchable text.");
        }

        await using MemoryStream parsed = new(Encoding.UTF8.GetBytes(text), writable: false);
        BlobDescriptor parsedBlob = await blobStoreService.PutAsync(parsed, new BlobWriteOptions(IngestionMediaTypes.Utf8PlainText), cancellationToken);
        EnsureSuccess(importJob.RecordParsedSnapshot(parsedBlob.Hash.Value));
        stopwatch.Stop();
        await usageMeterService.RecordAsync(importJob.WorkspaceId.Value, UsageMetricConstants.ExtractedBytes, Encoding.UTF8.GetByteCount(text), $"{importJob.Id.Value:N}:extracted", cancellationToken);
        await usageMeterService.RecordAsync(importJob.WorkspaceId.Value, UsageMetricConstants.ParserComputeMilliseconds, Math.Max(1, stopwatch.ElapsedMilliseconds), $"{importJob.Id.Value:N}:parse-ms", cancellationToken);
        CompleteStage(importJob, ImportPipelineStageType.Parse);
    }

    private async Task MaterializeArtifactAsync(ImportJob importJob, CancellationToken cancellationToken)
    {
        Source source = await GetSourceAsync(importJob, cancellationToken);
        string content = await ReadParsedTextAsync(importJob, cancellationToken);
        ArtifactId artifactId = ArtifactId.Create(DeterministicGuid(importJob.Id, ImportPipelineDiscriminators.Artifact));
        ArtifactRevisionId revisionId = ArtifactRevisionId.Create(DeterministicGuid(importJob.Id, ImportPipelineDiscriminators.Revision));

        DomainResult<ArtifactTitle> title = ArtifactTitle.Create(source.Name.Value);
        DomainResult<ArtifactContent> artifactContent = ArtifactContent.Create(content);
        EnsureSuccess(title);
        EnsureSuccess(artifactContent);

        ArtifactType artifactType = source.Type.Equals(SourceType.WebPage)
            ? ArtifactType.WebPage
            : source.Type.Equals(SourceType.Conversation)
                ? ArtifactType.Conversation
                : source.Definition is FileSourceDefinition file && (file.MediaType.Equals(IngestionMediaTypes.Markdown, StringComparison.OrdinalIgnoreCase) || Path.GetExtension(file.FileName).Equals(IngestionFileExtensions.Markdown, StringComparison.OrdinalIgnoreCase))
                    ? ArtifactType.Markdown
                    : source.Type.Equals(SourceType.PlainText) 
                        ? ArtifactType.Text 
                        : ArtifactType.File;

        DomainResult<Artifact> artifactResult = Artifact.Create(artifactId, importJob.WorkspaceId, title.Value, artifactType, clock.UtcNow);
        EnsureSuccess(artifactResult);
        Artifact artifact = artifactResult.Value;
        DomainResult<ArtifactRevision> revisionResult = artifact.CreateRevision(revisionId, artifactContent.Value, clock.UtcNow);
        EnsureSuccess(revisionResult);

        await artifactRepository.AddAsync(artifact, cancellationToken);
        await artifactRevisionRepository.AddAsync(revisionResult.Value, cancellationToken);
        EnsureSuccess(importJob.RecordMaterializedArtifact(artifactId, revisionId));
        CompleteStage(importJob, ImportPipelineStageType.MaterializeArtifact);
    }

    private async Task ChunkAsync(ImportJob importJob, CancellationToken cancellationToken)
    {
        ImportOptions options = ParseOptions(importJob.OptionsJson);
        if (!_chunkingStrategies.TryGetValue(options.ChunkingStrategy, out IChunkingStrategy? strategy))
        {
            throw Permanent(IngestionFailureCodes.UnsupportedChunkingStrategy, $"Chunking strategy '{options.ChunkingStrategy}' is not registered.");
        }

        string content = await ReadParsedTextAsync(importJob, cancellationToken);
        IReadOnlyList<ChunkSegment> segments = await strategy.ChunkAsync(content, options, cancellationToken);
        if (segments.Count == 0)
        {
            throw Permanent(IngestionFailureCodes.EmptyChunkBatch, "Chunking produced no chunks.");
        }

        ArtifactId artifactId = importJob.ArtifactId ?? throw Poison(IngestionFailureCodes.MissingArtifactReference, "Import has no artifact reference.");
        ArtifactRevisionId revisionId = importJob.ArtifactRevisionId ?? throw Poison(IngestionFailureCodes.MissingRevisionReference, "Import has no revision reference.");
        ChunkingStrategyType strategyType = Enumeration.GetAll<ChunkingStrategyType>().Single(value => value.Name.Equals(strategy.Name, StringComparison.OrdinalIgnoreCase));
        ChunkBatchId batchId = ChunkBatchId.Create(DeterministicGuid(importJob.Id, ImportPipelineDiscriminators.ChunkBatch));
        DomainResult<ChunkingVersion> version = ChunkingVersion.Create(ImportPipelineDiscriminators.ChunkingVersion);
        EnsureSuccess(version);
        DomainResult<ChunkBatch> batchResult = ChunkBatch.Request(batchId, importJob.WorkspaceId, artifactId, revisionId, strategyType, version.Value, clock.UtcNow);
        EnsureSuccess(batchResult);
        ChunkBatch batch = batchResult.Value;
        EnsureSuccess(batch.Start(clock.UtcNow));

        List<Chunk> chunks = new(segments.Count);
        foreach (ChunkSegment segment in segments)
        {
            DomainResult<ChunkNumber> number = ChunkNumber.Create(segment.Number);
            DomainResult<ChunkContent> chunkContent = ChunkContent.Create(segment.Content);
            DomainResult<SourceTextSpan> span = SourceTextSpan.Create(segment.Start, segment.Length);
            EnsureSuccess(number);
            EnsureSuccess(chunkContent);
            EnsureSuccess(span);
            DomainResult<Chunk> chunk = Chunk.Create(ChunkId.Create(DeterministicGuid(importJob.Id, $"chunk:{segment.Number}")), batch.Id, importJob.WorkspaceId, artifactId, revisionId, number.Value, chunkContent.Value, span.Value, strategyType, version.Value, clock.UtcNow);
            EnsureSuccess(chunk);
            chunks.Add(chunk.Value);
        }

        EnsureSuccess(batch.Complete(chunks.Count, clock.UtcNow));
        await chunkBatchRepository.AddAsync(batch, cancellationToken);
        await chunkRepository.AddRangeAsync(chunks, cancellationToken);
        EnsureSuccess(importJob.RecordChunkBatch(batch.Id));
        CompleteStage(importJob, ImportPipelineStageType.Chunk);
    }

    private async Task EmbedAndIndexAsync(ImportJob importJob, CancellationToken cancellationToken)
    {
        ImportOptions options = ParseOptions(importJob.OptionsJson);
        (string identifier, string version) = ParseModel(options.EmbeddingModel);
        ArtifactRevisionId revisionId = importJob.ArtifactRevisionId ?? throw Poison(IngestionFailureCodes.MissingRevisionReference, "Import has no revision reference.");
        IReadOnlyList<Chunk> chunks = await chunkRepository.ListByArtifactRevisionIdAsync(revisionId, cancellationToken);
        DomainResult<EmbeddingModel> model = EmbeddingModel.Create(identifier, version);
        EnsureSuccess(model);
        IReadOnlyList<int> storedDimensions = await chunkEmbeddingRepository.ListDimensionsAsync(importJob.WorkspaceId, model.Value, cancellationToken);
        if (storedDimensions.Count > 1)
        {
            throw Permanent(IngestionFailureCodes.EmbeddingDimensionMismatch, "Stored vectors for the configured model have inconsistent dimensions.");
        }
        int? expectedDimensions = storedDimensions.SingleOrDefault();
        long inputUnits = 0;

        for (int offset = 0; offset < chunks.Count; offset += EmbeddingBatchSize)
        {
            Chunk[] batch = chunks.Skip(offset).Take(EmbeddingBatchSize).ToArray();
            IReadOnlyList<GeneratedEmbedding> generated = await embeddingGenerator.GenerateBatchAsync(identifier, version, batch.Select(chunk => chunk.Content.Value).ToArray(), cancellationToken);
            inputUnits += generated.Sum(item => item.InputUnits);
            if (generated.Count != batch.Length)
            {
                throw new IngestionException(JobFailureCategoryType.Transient, IngestionFailureCodes.EmbeddingCountMismatch, "Embedding provider returned a different number of vectors than inputs.");
            }

            int? dimensions = null;
            for (int index = 0; index < batch.Length; index++)
            {
                Chunk chunk = batch[index];
                IReadOnlyList<float> vector = generated[index].Vector;
                if (vector.Count == 0 || vector.Any(value => !float.IsFinite(value)))
                {
                    throw Permanent(IngestionFailureCodes.InvalidEmbeddingVector, "Embedding vector is empty or non-finite.");
                }

                dimensions ??= vector.Count;
                if (dimensions != vector.Count || expectedDimensions is > 0 && expectedDimensions != vector.Count)
                {
                    throw Permanent(IngestionFailureCodes.EmbeddingDimensionMismatch, "Embedding provider returned inconsistent dimensions.");
                }

                DomainResult<EmbeddingDimensions> embeddingDimensions = EmbeddingDimensions.Create(vector.Count);
                EnsureSuccess(embeddingDimensions);
                ChunkEmbeddingId embeddingId = ChunkEmbeddingId.Create(DeterministicGuid(importJob.Id, $"embedding:{chunk.Number.Value}:{identifier}:{version}"));
                DomainResult<ChunkEmbedding> embedding = ChunkEmbedding.Create(embeddingId, importJob.WorkspaceId, chunk.Id, chunk.ContentHash, model.Value, embeddingDimensions.Value, clock.UtcNow);
                EnsureSuccess(embedding);
                await chunkEmbeddingRepository.AddAsync(embedding.Value, cancellationToken);
                await embeddingVectorStore.UpsertAsync(embeddingId, vector, cancellationToken);
            }
        }

        await usageMeterService.RecordAsync(importJob.WorkspaceId.Value, UsageMetricConstants.EmbeddingInputUnits, inputUnits, $"{importJob.Id.Value:N}:embedding-input", cancellationToken);

        CompleteStage(importJob, ImportPipelineStageType.EmbedAndIndex);
    }

    private async Task CompleteAsync(ImportJob importJob, CancellationToken cancellationToken)
    {
        ArtifactId artifactId = importJob.ArtifactId ?? throw Poison(IngestionFailureCodes.MissingArtifactReference, "Import has no artifact reference.");
        ArtifactRevisionId revisionId = importJob.ArtifactRevisionId ?? throw Poison(IngestionFailureCodes.MissingRevisionReference, "Import has no revision reference.");
        EnsureSuccess(importJob.Complete(artifactId, revisionId, clock.UtcNow));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (importJob.ParsedBlobHash is not null 
            && !string.Equals(importJob.ParsedBlobHash, importJob.RawBlobHash, StringComparison.Ordinal)
            && !await importJobRepository.IsBlobReferencedByOtherImportAsync(importJob.Id, importJob.ParsedBlobHash, cancellationToken))
        {
            await blobStoreService.DeleteAsync(new BlobHash(importJob.ParsedBlobHash), cancellationToken);
        }
    }

    private async Task<Source> GetSourceAsync(ImportJob importJob, CancellationToken cancellationToken)
    {
        Source source = await sourceRepository.GetByIdAsync(importJob.SourceId, cancellationToken) ?? throw Permanent(IngestionFailureCodes.SourceNotFound, "Source no longer exists.");
        return source.WorkspaceId != importJob.WorkspaceId ? throw Permanent(IngestionFailureCodes.SourceNotFound, "Source is outside the import workspace.") : source;
    }

    private async Task<string> ReadParsedTextAsync(ImportJob importJob, CancellationToken cancellationToken)
    {
        BlobHash hash = ParseBlobHash(importJob.ParsedBlobHash, ImportPipelineDiscriminators.ParsedBlob);
        await using Stream stream = await blobStoreService.OpenReadAsync(hash, cancellationToken);
        using StreamReader reader = new(stream, Encoding.UTF8, true, leaveOpen: true);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static (string FileName, string MediaType) GetSourceMetadata(SourceDefinition definition) =>
        definition switch
        {
            FileSourceDefinition file => (file.FileName, file.MediaType),
            WebPageSourceDefinition => ("page.html", IngestionMediaTypes.Html),
            PlainTextSourceDefinition text => (text.Title, IngestionMediaTypes.PlainText),
            ConversationSourceDefinition conversation => (conversation.Title, IngestionMediaTypes.PlainText),
            _ => ("source.txt", IngestionMediaTypes.PlainText)
        };

    private static ImportOptions ParseOptions(string json) =>
        JsonSerializer.Deserialize<ImportOptions>(json, JsonOptions) ?? throw Poison(IngestionFailureCodes.InvalidImportOptions, "Import options payload is malformed.");

    private static (string Identifier, string Version) ParseModel(string? model)
    {
        string[] parts = model?.Split('@', 2, StringSplitOptions.TrimEntries) ?? [];
        return parts.Length == 2 && parts.All(part => part.Length > 0)
            ? (parts[0], parts[1])
            : throw Permanent(IngestionFailureCodes.InvalidEmbeddingModel, "Embedding model must use 'identifier@version' format.");
    }

    private static BlobHash ParseBlobHash(string? value, string kind) =>
        value is null ? throw Poison($"missing_{kind}_blob", $"Import has no {kind} blob reference.") : new BlobHash(value);

    private void CompleteStage(ImportJob importJob, ImportPipelineStageType stage) =>
        EnsureSuccess(importJob.CompleteStage(stage, clock.UtcNow));

    private static Guid DeterministicGuid(ImportJobId importJobId, string discriminator)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{importJobId.Value:N}:{discriminator}"));
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

    private static IngestionException Permanent(string code, string message) =>
        new(JobFailureCategoryType.Permanent, code, message);

    private static IngestionException Poison(string code, string message) =>
        new(JobFailureCategoryType.Poison, code, message);
}