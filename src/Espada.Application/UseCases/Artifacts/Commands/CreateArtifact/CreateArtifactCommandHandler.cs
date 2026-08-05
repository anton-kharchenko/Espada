using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.Models;
using Espada.Application.Rules;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Artifacts.Commands.CreateArtifact
{
    internal sealed class CreateArtifactCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IArtifactRepository artifactRepository,
        IArtifactRevisionRepository artifactRevisionRepository,
        IInstructionRuleRepository instructionRuleRepository,
        IPolicyRuleRepository policyRuleRepository,
        IUnitOfWork unitOfWork,
        IClockService clockService,
        IMapper mapper)
        : ICommandHandler<CreateArtifactCommand, CreateArtifactResponse>
    {
        public async Task<DomainResult<CreateArtifactResponse>> Handle(
            CreateArtifactCommand request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure<CreateArtifactResponse>(WorkspaceApplicationErrors.InvalidId);
            }

            DomainResult<ArtifactTitle> titleResult = ArtifactTitle.Create(request.Title);
            DomainResult<ArtifactContent> contentResult = ArtifactContent.Create(request.Content);
            ArtifactType? artifactType = Enumeration
                .GetAll<ArtifactType>()
                .SingleOrDefault(value => value.Id == request.TypeId);
            ArtifactKindType? kindType = Enumeration
                .GetAll<ArtifactKindType>()
                .SingleOrDefault(value => value.Id == request.KindTypeId);

            if (titleResult.IsFailure || contentResult.IsFailure || artifactType is null || kindType is null)
            {
                DomainError error = titleResult.IsFailure
                    ? titleResult.Error
                    : contentResult.IsFailure
                        ? contentResult.Error
                        : artifactType is null
                            ? ArtifactApplicationErrors.UnsupportedType(request.TypeId)
                            : ArtifactApplicationErrors.UnsupportedKindType(request.KindTypeId);
                return DomainResult.Failure<CreateArtifactResponse>(error);
            }

            if (kindType.Equals(ArtifactKindType.Memory))
            {
                return DomainResult.Failure<CreateArtifactResponse>(
                    ArtifactApplicationErrors.MemoryRequiresRememberCommand);
            }

            if (kindType.Equals(ArtifactKindType.Policy)
                && !request.AllowPolicyMutation)
            {
                return DomainResult.Failure<CreateArtifactResponse>(
                    ArtifactApplicationErrors
                        .PolicyMutationRequiresAdministrator);
            }

            Workspace? workspace = await workspaceRepository.GetByIdAsync(
                WorkspaceId.Create(request.WorkspaceId),
                cancellationToken);
            if (workspace is null)
            {
                return DomainResult.Failure<CreateArtifactResponse>(
                    WorkspaceApplicationErrors.NotFound(request.WorkspaceId));
            }

            DateTimeOffset createdAtUtc = clockService.UtcNow;
            DomainResult<Artifact> artifactResult = request.IsDraft
                ? Artifact.CreateDraft(
                    ArtifactId.Create(Guid.NewGuid()),
                    workspace.Id,
                    titleResult.Value,
                    kindType,
                    artifactType,
                    createdAtUtc)
                : Artifact.Create(
                    ArtifactId.Create(Guid.NewGuid()),
                    workspace.Id,
                    titleResult.Value,
                    kindType,
                    artifactType,
                    createdAtUtc);
            if (artifactResult.IsFailure)
            {
                return DomainResult.Failure<CreateArtifactResponse>(artifactResult.Error);
            }

            Artifact artifact = artifactResult.Value;
            DomainResult<ArtifactRevision> revisionResult = artifact.CreateRevision(
                ArtifactRevisionId.Create(Guid.NewGuid()),
                contentResult.Value,
                createdAtUtc);
            if (revisionResult.IsFailure)
            {
                return DomainResult.Failure<CreateArtifactResponse>(revisionResult.Error);
            }

            ArtifactRevision revision = revisionResult.Value;
            DomainResult<ArtifactRuleSet> ruleSetResult = ArtifactRuleFactory.Create(
                artifact,
                revision,
                request.InstructionRules,
                request.PolicyRules);
            if (ruleSetResult.IsFailure)
            {
                return DomainResult.Failure<CreateArtifactResponse>(ruleSetResult.Error);
            }

            await artifactRepository.AddAsync(artifact, cancellationToken);
            await artifactRevisionRepository.AddAsync(revision, cancellationToken);
            await instructionRuleRepository.AddRangeAsync(
                ruleSetResult.Value.InstructionRules,
                cancellationToken);
            await policyRuleRepository.AddRangeAsync(
                ruleSetResult.Value.PolicyRules,
                cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            CreateArtifactResponse response = mapper.Map<CreateArtifactResponse>(
                new ArtifactRevisionResponseMappingSource(artifact, revision));

            return DomainResult.Success(response);
        }
    }
}