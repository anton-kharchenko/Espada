using Espada.Application.Constants;
using Espada.Application.Contracts.Billing;
using Espada.Application.Contracts.Billing.Constants;
using Espada.Application.Contracts.Blobs;
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
using Espada.Domain.ValueObjects;
using Espada.Domain.ValueObjects.SourceDefinitions;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Espada.Application.Services
{
    internal sealed class ImportPipelineStageExecutorService(
        IImportJobRepository importJobRepository,
        ISourceRepository sourceRepository,
        IArtifactRepository artifactRepository,
        IArtifactRevisionRepository artifactRevisionRepository,
        ArtifactIndexingService artifactIndexingService,
        ISourceReader sourceReader,
        ISourceParser sourceParser,
        IBlobStoreService blobStoreService,
        IUnitOfWork unitOfWork,
        IClockService clock,
        IUsageMeterService usageMeterService)
        : IImportPipelineStageExecutorService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public async Task ExecuteAsync(IngestionJob job, CancellationToken cancellationToken = default)
        {
            ImportJob importJob = await importJobRepository.GetByIdAsync(job.ImportJobId, cancellationToken) ??
                                  throw Permanent(IngestionFailureCodeConstants.ImportNotFound, "Import no longer exists.");

            if (importJob.Status.Equals(ImportStatusType.Cancelled))
            {
                throw new OperationCanceledException("Import was cancelled.", cancellationToken);
            }

            if (importJob.Status.Equals(ImportStatusType.Succeeded) ||
                importJob.Status.Equals(ImportStatusType.Failed) || job.Stage.Id < importJob.Stage.Id)
            {
                return;
            }

            if (!job.Stage.Equals(importJob.Stage))
            {
                throw new IngestionException(JobFailureCategoryType.Poison, IngestionFailureCodeConstants.StageMismatch,
                    $"Job stage '{job.Stage}' does not match import stage '{importJob.Stage}'.");
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
                throw new IngestionException(JobFailureCategoryType.Poison, IngestionFailureCodeConstants.UnknownStage,
                    $"Unknown import stage '{job.Stage}'.");
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
                BlobDescriptor raw = await blobStoreService.PutAsync(read.Content, new BlobWriteOptions(read.MediaType),
                    cancellationToken);
                EnsureSuccess(importJob.RecordRawSnapshot(raw.Hash.Value));
                await usageMeterService.RecordAsync(importJob.WorkspaceId.Value, UsageMetricConstants.RawBytes,
                    raw.Length, $"{importJob.Id.Value:N}:raw", cancellationToken);
            }

            stopwatch.Stop();
            await usageMeterService.RecordAsync(importJob.WorkspaceId.Value,
                source.Definition is ConnectorSourceDefinition
                    ? UsageMetricConstants.PluginComputeMilliseconds
                    : UsageMetricConstants.ParserComputeMilliseconds, Math.Max(1, stopwatch.ElapsedMilliseconds),
                $"{importJob.Id.Value:N}:read-ms", cancellationToken);

            CompleteStage(importJob, ImportPipelineStageType.Read);
        }

        private async Task ParseAsync(ImportJob importJob, CancellationToken cancellationToken)
        {
            Source source = await GetSourceAsync(importJob, cancellationToken);
            Stopwatch stopwatch = Stopwatch.StartNew();
            BlobHash rawHash = ParseBlobHash(importJob.RawBlobHash, ImportPipelineDiscriminatorConstants.RawBlob);
            await using Stream raw = await blobStoreService.OpenReadAsync(rawHash, cancellationToken);
            (string fileName, string mediaType) = GetSourceMetadata(source.Definition);
            string text = await sourceParser.ParseAsync(raw, fileName, mediaType, cancellationToken);
            if (string.IsNullOrWhiteSpace(text))
            {
                throw Permanent(IngestionFailureCodeConstants.EmptyExtractedText, "Source produced no searchable text.");
            }

            await using MemoryStream parsed = new(Encoding.UTF8.GetBytes(text), false);
            BlobDescriptor parsedBlob = await blobStoreService.PutAsync(parsed,
                new BlobWriteOptions(IngestionMediaTypeConstants.Utf8PlainText), cancellationToken);
            EnsureSuccess(importJob.RecordParsedSnapshot(parsedBlob.Hash.Value));
            stopwatch.Stop();
            await usageMeterService.RecordAsync(importJob.WorkspaceId.Value, UsageMetricConstants.ExtractedBytes,
                Encoding.UTF8.GetByteCount(text), $"{importJob.Id.Value:N}:extracted", cancellationToken);
            await usageMeterService.RecordAsync(importJob.WorkspaceId.Value,
                UsageMetricConstants.ParserComputeMilliseconds, Math.Max(1, stopwatch.ElapsedMilliseconds),
                $"{importJob.Id.Value:N}:parse-ms", cancellationToken);
            CompleteStage(importJob, ImportPipelineStageType.Parse);
        }

        private async Task MaterializeArtifactAsync(ImportJob importJob, CancellationToken cancellationToken)
        {
            Source source = await GetSourceAsync(importJob, cancellationToken);
            string content = await ReadParsedTextAsync(importJob, cancellationToken);
            ArtifactId artifactId =
                ArtifactId.Create(DeterministicGuid(importJob.Id, ImportPipelineDiscriminatorConstants.Artifact));
            ArtifactRevisionId revisionId =
                ArtifactRevisionId.Create(DeterministicGuid(importJob.Id, ImportPipelineDiscriminatorConstants.Revision));

            DomainResult<ArtifactTitle> title = ArtifactTitle.Create(source.Name.Value);
            DomainResult<ArtifactContent> artifactContent = ArtifactContent.Create(content);
            EnsureSuccess(title);
            EnsureSuccess(artifactContent);

            ArtifactType artifactType = source.Type.Equals(SourceType.WebPage)
                ? ArtifactType.WebPage
                : source.Type.Equals(SourceType.Conversation)
                    ? ArtifactType.Conversation
                    : source.Definition is FileSourceDefinition file &&
                      (file.MediaType.Equals(IngestionMediaTypeConstants.Markdown, StringComparison.OrdinalIgnoreCase) ||
                       Path.GetExtension(file.FileName).Equals(IngestionFileExtensionConstants.Markdown,
                           StringComparison.OrdinalIgnoreCase))
                        ? ArtifactType.Markdown
                        : source.Type.Equals(SourceType.PlainText)
                            ? ArtifactType.Text
                            : ArtifactType.File;

            DomainResult<Artifact> artifactResult = Artifact.Create(artifactId, importJob.WorkspaceId, title.Value,
                ArtifactKindType.Document, artifactType, clock.UtcNow);
            EnsureSuccess(artifactResult);
            Artifact artifact = artifactResult.Value;
            DomainResult<ArtifactRevision> revisionResult =
                artifact.CreateRevision(revisionId, artifactContent.Value, clock.UtcNow);
            EnsureSuccess(revisionResult);

            await artifactRepository.AddAsync(artifact, cancellationToken);
            await artifactRevisionRepository.AddAsync(revisionResult.Value, cancellationToken);
            EnsureSuccess(importJob.RecordMaterializedArtifact(artifactId, revisionId));
            CompleteStage(importJob, ImportPipelineStageType.MaterializeArtifact);
        }

        private async Task ChunkAsync(
            ImportJob importJob,
            CancellationToken cancellationToken)
        {
            ImportOptions options = ParseOptions(importJob.OptionsJson);
            string content = await ReadParsedTextAsync(importJob, cancellationToken);
            ArtifactId artifactId = importJob.ArtifactId ?? throw Poison(
                IngestionFailureCodeConstants.MissingArtifactReference,
                "Import has no artifact reference.");
            ArtifactRevisionId revisionId = importJob.ArtifactRevisionId ?? throw Poison(
                IngestionFailureCodeConstants.MissingRevisionReference,
                "Import has no revision reference.");

            ArtifactChunkingResult chunkingResult = await artifactIndexingService.ChunkAsync(
                importJob.Id.Value,
                importJob.WorkspaceId,
                artifactId,
                revisionId,
                content,
                options,
                cancellationToken);
            EnsureSuccess(importJob.RecordChunkBatch(chunkingResult.Batch.Id));
            CompleteStage(importJob, ImportPipelineStageType.Chunk);
        }

        private async Task EmbedAndIndexAsync(
            ImportJob importJob,
            CancellationToken cancellationToken)
        {
            ImportOptions options = ParseOptions(importJob.OptionsJson);
            ArtifactRevisionId revisionId = importJob.ArtifactRevisionId ?? throw Poison(
                IngestionFailureCodeConstants.MissingRevisionReference,
                "Import has no revision reference.");

            await artifactIndexingService.EmbedAndIndexAsync(
                importJob.Id.Value,
                importJob.WorkspaceId,
                revisionId,
                options.EmbeddingModel ?? string.Empty,
                $"{importJob.Id.Value:N}:embedding-input",
                cancellationToken);
            CompleteStage(importJob, ImportPipelineStageType.EmbedAndIndex);
        }

        private async Task CompleteAsync(ImportJob importJob, CancellationToken cancellationToken)
        {
            ArtifactId artifactId = importJob.ArtifactId ?? throw Poison(IngestionFailureCodeConstants.MissingArtifactReference,
                "Import has no artifact reference.");
            ArtifactRevisionId revisionId = importJob.ArtifactRevisionId ??
                                            throw Poison(IngestionFailureCodeConstants.MissingRevisionReference,
                                                "Import has no revision reference.");
            EnsureSuccess(importJob.Complete(artifactId, revisionId, clock.UtcNow));
            await unitOfWork.SaveChangesAsync(cancellationToken);

            if (importJob.ParsedBlobHash is not null
                && !string.Equals(importJob.ParsedBlobHash, importJob.RawBlobHash, StringComparison.Ordinal)
                && !await importJobRepository.IsBlobReferencedByOtherImportAsync(importJob.Id, importJob.ParsedBlobHash,
                    cancellationToken))
            {
                await blobStoreService.DeleteAsync(new BlobHash(importJob.ParsedBlobHash), cancellationToken);
            }
        }

        private async Task<Source> GetSourceAsync(ImportJob importJob, CancellationToken cancellationToken)
        {
            Source source = await sourceRepository.GetByIdAsync(importJob.SourceId, cancellationToken) ??
                            throw Permanent(IngestionFailureCodeConstants.SourceNotFound, "Source no longer exists.");
            return source.WorkspaceId != importJob.WorkspaceId
                ? throw Permanent(IngestionFailureCodeConstants.SourceNotFound, "Source is outside the import workspace.")
                : source;
        }

        private async Task<string> ReadParsedTextAsync(ImportJob importJob, CancellationToken cancellationToken)
        {
            BlobHash hash = ParseBlobHash(importJob.ParsedBlobHash, ImportPipelineDiscriminatorConstants.ParsedBlob);
            await using Stream stream = await blobStoreService.OpenReadAsync(hash, cancellationToken);
            using StreamReader reader = new(stream, Encoding.UTF8, leaveOpen: true);
            return await reader.ReadToEndAsync(cancellationToken);
        }

        private static (string FileName, string MediaType) GetSourceMetadata(SourceDefinition definition)
        {
            return definition switch
            {
                FileSourceDefinition file => (file.FileName, file.MediaType),
                WebPageSourceDefinition => ("page.html", IngestionMediaTypeConstants.Html),
                PlainTextSourceDefinition text => (text.Title, IngestionMediaTypeConstants.PlainText),
                ConversationSourceDefinition conversation => (conversation.Title, IngestionMediaTypeConstants.PlainText),
                _ => ("source.txt", IngestionMediaTypeConstants.PlainText)
            };
        }

        private static ImportOptions ParseOptions(string json)
        {
            return JsonSerializer.Deserialize<ImportOptions>(json, JsonOptions) ??
                   throw Poison(IngestionFailureCodeConstants.InvalidImportOptions, "Import options payload is malformed.");
        }

        private static BlobHash ParseBlobHash(string? value, string kind)
        {
            return value is null
                ? throw Poison($"missing_{kind}_blob", $"Import has no {kind} blob reference.")
                : new BlobHash(value);
        }

        private void CompleteStage(ImportJob importJob, ImportPipelineStageType stage)
        {
            EnsureSuccess(importJob.CompleteStage(stage, clock.UtcNow));
        }

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

        private static IngestionException Permanent(string code, string message)
        {
            return new IngestionException(JobFailureCategoryType.Permanent, code, message);
        }

        private static IngestionException Poison(string code, string message)
        {
            return new IngestionException(JobFailureCategoryType.Poison, code, message);
        }
    }
}