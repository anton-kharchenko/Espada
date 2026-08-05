using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Billing;
using Espada.Application.Contracts.Embedding;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Repositories;
using Espada.Application.Contracts.Time;
using Espada.Application.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Espada.Domain.ValueObjects.SourceDefinitions;
using System.Security.Cryptography;
using System.Text;

namespace Espada.Application.UseCases.Imports.Commands.RequestImport
{
    internal sealed class RequestImportCommandHandler(
        ISourceRepository sourceRepository,
        IProjectRepository projectRepository,
        IImportJobRepository importJobRepository,
        IRepositoryScanner repositoryScanner,
        IRepositoryManifestStore repositoryManifestStore,
        IUnitOfWork unitOfWork,
        IClockService clockService,
        IImportAdmissionPolicy importAdmissionPolicy,
        IEmbeddingModelDefaults embeddingModelDefaults) : ICommandHandler<RequestImportCommand, RequestImportResponse>
    {
        public async Task<DomainResult<RequestImportResponse>> Handle(RequestImportCommand request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure<RequestImportResponse>(WorkspaceApplicationErrors.InvalidId);
            }

            if (request.SourceId == Guid.Empty)
            {
                return DomainResult.Failure<RequestImportResponse>(SourceApplicationErrors.InvalidId);
            }

            ImportOptions options = request.Options with
            {
                EmbeddingModel = request.Options.EmbeddingModel ?? embeddingModelDefaults.DefaultModel
            };

            string? denialReason =
                await importAdmissionPolicy.GetDenialReasonAsync(request.WorkspaceId, cancellationToken);
            if (denialReason is not null)
            {
                return DomainResult.Failure<RequestImportResponse>(
                    ImportJobApplicationErrors.CloudImportBlocked(denialReason));
            }

            Source? source = await sourceRepository.GetByIdAsync(SourceId.Create(request.SourceId), cancellationToken);
            if (source is null)
            {
                return DomainResult.Failure<RequestImportResponse>(SourceApplicationErrors.NotFound(request.SourceId));
            }

            if (source.WorkspaceId.Value != request.WorkspaceId)
            {
                return DomainResult.Failure<RequestImportResponse>(
                    SourceApplicationErrors.NotFoundInWorkspace(request.SourceId, request.WorkspaceId));
            }

            return source.Definition is RepositorySourceDefinition repository
                ? await RequestRepositoryImportAsync(request, options, source, repository, cancellationToken)
                : await RequestSingleImportAsync(request, options, source, cancellationToken);
        }

        private async Task<DomainResult<RequestImportResponse>> RequestSingleImportAsync(RequestImportCommand request,
            ImportOptions options, Source source, CancellationToken cancellationToken)
        {
            if (options.RepositoryFile is not null)
            {
                return DomainResult.Failure<RequestImportResponse>(ImportJobApplicationErrors.IdempotencyConflict);
            }

            string requestFingerprint = RequestImportFingerprint.Create(request.SourceId, options);
            ImportJob? existing = await importJobRepository.GetByIdempotencyKeyAsync(source.WorkspaceId,
                request.IdempotencyKey, cancellationToken);
            if (existing is not null)
            {
                return existing.RequestFingerprint == requestFingerprint
                    ? DomainResult.Success(new RequestImportResponse(existing.Id.Value, [existing.Id.Value]))
                    : DomainResult.Failure<RequestImportResponse>(ImportJobApplicationErrors.IdempotencyConflict);
            }

            ImportJob importJob = CreateImportJob(source, request.IdempotencyKey, requestFingerprint, options);
            await importJobRepository.AddAsync(importJob, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return DomainResult.Success(new RequestImportResponse(importJob.Id.Value, [importJob.Id.Value]));
        }

        private async Task<DomainResult<RequestImportResponse>> RequestRepositoryImportAsync(
            RequestImportCommand request, ImportOptions options, Source source, RepositorySourceDefinition repository,
            CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(repository.RepositoryIdentity, out Guid projectIdValue))
            {
                return DomainResult.Failure<RequestImportResponse>(
                    ImportJobApplicationErrors.RepositoryIdentityInvalid);
            }

            Project? project = await projectRepository.GetByIdAsync(ProjectId.Create(projectIdValue), cancellationToken);
            if (project is null || project.WorkspaceId != source.WorkspaceId)
            {
                return DomainResult.Failure<RequestImportResponse>(
                    ProjectApplicationErrors.NotFoundInWorkspace(projectIdValue, source.WorkspaceId.Value));
            }

            DomainResult<RepositoryScanResult> scanResult = await repositoryScanner.ScanAsync(
                project.LocalAliases, repository.ScanPolicy, cancellationToken);
            if (scanResult.IsFailure)
            {
                return DomainResult.Failure<RequestImportResponse>(scanResult.Error);
            }

            IReadOnlyDictionary<string, string> currentHashes =
                await repositoryManifestStore.LoadHashesAsync(source.Id, cancellationToken);
            RepositoryFileRecord[] changedFiles = scanResult.Value.Files
                .Where(file => !currentHashes.TryGetValue(file.RelativePath, out string? hash) || hash != file.ContentHash)
                .OrderBy(file => file.RelativePath, StringComparer.Ordinal)
                .ToArray();
            List<Guid> workItemIds = [];
            foreach (RepositoryFileRecord file in changedFiles)
            {
                ImportOptions fileOptions = options with
                {
                    RepositoryFile = new RepositoryFileImportOptions(scanResult.Value.RepositoryRoot,
                        file.RelativePath, file.ContentHash, file.FileName, file.MediaType, file.SizeInBytes)
                };
                string idempotencyKey = CreateRepositoryIdempotencyKey(request.IdempotencyKey, file);
                string fingerprint = RequestImportFingerprint.Create(request.SourceId, fileOptions);
                ImportJob? existing = await importJobRepository.GetByIdempotencyKeyAsync(source.WorkspaceId,
                    idempotencyKey, cancellationToken);
                if (existing is not null)
                {
                    if (existing.RequestFingerprint != fingerprint)
                    {
                        return DomainResult.Failure<RequestImportResponse>(
                            ImportJobApplicationErrors.IdempotencyConflict);
                    }

                    workItemIds.Add(existing.Id.Value);
                    continue;
                }

                ImportJob importJob = CreateImportJob(source, idempotencyKey, fingerprint, fileOptions);
                await importJobRepository.AddAsync(importJob, cancellationToken);
                workItemIds.Add(importJob.Id.Value);
            }

            await repositoryManifestStore.ReplaceAsync(source.Id, scanResult.Value.Files, clockService.UtcNow,
                cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return DomainResult.Success(new RequestImportResponse(workItemIds.Count == 0 ? null : workItemIds[0], workItemIds));
        }

        private ImportJob CreateImportJob(Source source, string idempotencyKey, string requestFingerprint,
            ImportOptions options)
        {
            return ImportJob.Request(ImportJobId.New(), source.Id, source.WorkspaceId, clockService.UtcNow,
                idempotencyKey, requestFingerprint, RequestImportFingerprint.SerializeOptions(options)).Value;
        }

        private static string CreateRepositoryIdempotencyKey(string requestKey, RepositoryFileRecord file)
        {
            return Convert.ToHexStringLower(SHA256.HashData(
                Encoding.UTF8.GetBytes($"{requestKey}:{file.RelativePath}:{file.ContentHash}")));
        }
    }
}