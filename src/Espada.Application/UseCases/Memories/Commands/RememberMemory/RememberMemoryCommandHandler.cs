using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Embedding;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.Services;
using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Memories.Commands.RememberMemory
{
    internal sealed class RememberMemoryCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IArtifactRepository artifactRepository,
        IArtifactRevisionRepository artifactRevisionRepository,
        IMemoryMetadataRepository memoryMetadataRepository,
        IBindingRepository bindingRepository,
        IEmbeddingModelDefaults embeddingModelDefaults,
        ArtifactIndexingService artifactIndexingService,
        IUnitOfWork unitOfWork,
        IClockService clockService,
        IMapper mapper)
        : ICommandHandler<RememberMemoryCommand, RememberMemoryResponse>
    {
        public async Task<DomainResult<RememberMemoryResponse>> Handle(
            RememberMemoryCommand request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure<RememberMemoryResponse>(WorkspaceApplicationErrors.InvalidId);
            }

            DomainResult<ArtifactTitle> titleResult = ArtifactTitle.Create(request.Title);
            DomainResult<ArtifactContent> contentResult = ArtifactContent.Create(request.Content);
            MemoryCategoryType? categoryType = Enumeration
                .GetAll<MemoryCategoryType>()
                .SingleOrDefault(value => value.Id == request.CategoryTypeId);
            if (titleResult.IsFailure || contentResult.IsFailure || categoryType is null)
            {
                DomainError error = titleResult.IsFailure
                    ? titleResult.Error
                    : contentResult.IsFailure
                        ? contentResult.Error
                        : MemoryApplicationErrors.UnsupportedCategoryType(request.CategoryTypeId);
                return DomainResult.Failure<RememberMemoryResponse>(error);
            }

            string? embeddingModel = embeddingModelDefaults.DefaultModel;

            Workspace? workspace = await workspaceRepository.GetByIdAsync(
                WorkspaceId.Create(request.WorkspaceId),
                cancellationToken);
            if (workspace is null)
            {
                return DomainResult.Failure<RememberMemoryResponse>(
                    WorkspaceApplicationErrors.NotFound(request.WorkspaceId));
            }

            MemoryId? supersededMemoryId = request.SupersededMemoryId.HasValue
                ? MemoryId.Create(request.SupersededMemoryId.Value)
                : null;
            if (supersededMemoryId is not null)
            {
                DomainResult validationResult = await ValidateSupersededMemoryAsync(
                    workspace,
                    supersededMemoryId,
                    cancellationToken);
                if (validationResult.IsFailure)
                {
                    return DomainResult.Failure<RememberMemoryResponse>(validationResult.Error);
                }
            }

            DateTimeOffset capturedAtUtc = clockService.UtcNow;
            Artifact artifact = Artifact.Create(
                ArtifactId.Create(Guid.NewGuid()),
                workspace.Id,
                titleResult.Value,
                ArtifactKindType.Memory,
                ArtifactType.Text,
                capturedAtUtc).Value;
            ArtifactRevision revision = artifact.CreateRevision(
                ArtifactRevisionId.Create(Guid.NewGuid()),
                contentResult.Value,
                capturedAtUtc).Value;
            MemoryId memoryId = MemoryId.Create(Guid.NewGuid());
            DomainResult<MemoryMetadata> metadataResult = artifact.CreateMemoryMetadata(
                memoryId,
                revision,
                categoryType,
                request.Confidence,
                false,
                request.ClientIdentity,
                request.SessionIdentity,
                capturedAtUtc,
                supersededMemoryId);
            if (metadataResult.IsFailure)
            {
                return DomainResult.Failure<RememberMemoryResponse>(metadataResult.Error);
            }

            DomainResult<Binding> bindingResult = artifact.CreateBinding(
                BindingId.New(),
                revision,
                workspace,
                workspace.OrganizationId,
                null,
                null,
                null,
                null,
                null,
                null,
                capturedAtUtc);
            if (bindingResult.IsFailure)
            {
                return DomainResult.Failure<RememberMemoryResponse>(
                    bindingResult.Error);
            }

            await artifactRepository.AddAsync(artifact, cancellationToken);
            await artifactRevisionRepository.AddAsync(revision, cancellationToken);
            await memoryMetadataRepository.AddAsync(metadataResult.Value, cancellationToken);
            await bindingRepository.UpsertAsync(
                bindingResult.Value,
                cancellationToken);
            await artifactIndexingService.IndexAsync(
                memoryId.Value,
                workspace.Id,
                artifact.Id,
                revision.Id,
                revision.Content.Value,
                new ImportOptions(embeddingModel),
                $"{memoryId.Value:N}:embedding-input",
                cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return DomainResult.Success(
                mapper.Map<RememberMemoryResponse>(metadataResult.Value));
        }

        private async Task<DomainResult> ValidateSupersededMemoryAsync(
            Workspace workspace,
            MemoryId supersededMemoryId,
            CancellationToken cancellationToken)
        {
            MemoryMetadata? supersededMemory = await memoryMetadataRepository.GetByIdAsync(
                supersededMemoryId,
                cancellationToken);
            if (supersededMemory is null)
            {
                return DomainResult.Failure(MemoryApplicationErrors.NotFound(supersededMemoryId.Value));
            }

            Artifact? supersededArtifact = await artifactRepository.GetByIdAsync(
                supersededMemory.ArtifactId,
                cancellationToken);
            if (supersededArtifact is null || !supersededArtifact.WorkspaceId.Equals(workspace.Id))
            {
                return DomainResult.Failure(
                    MemoryApplicationErrors.NotFoundInWorkspace(
                        supersededMemoryId.Value,
                        workspace.Id.Value));
            }

            if (await memoryMetadataRepository.IsSupersededAsync(
                    supersededMemoryId,
                    cancellationToken))
            {
                return DomainResult.Failure(
                    MemoryApplicationErrors.AlreadySuperseded(supersededMemoryId.Value));
            }

            return DomainResult.Success();
        }
    }
}