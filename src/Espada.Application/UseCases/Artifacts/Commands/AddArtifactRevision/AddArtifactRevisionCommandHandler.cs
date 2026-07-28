using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Models;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.UseCases.Artifacts.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Artifacts.Commands.AddArtifactRevision
{
    internal sealed class AddArtifactRevisionCommandHandler(
        IArtifactRepository artifactRepository,
        IArtifactRevisionRepository artifactRevisionRepository,
        IInstructionRuleRepository instructionRuleRepository,
        IPolicyRuleRepository policyRuleRepository,
        IUnitOfWork unitOfWork,
        IClockService clockService,
        IMapper mapper)
        : ICommandHandler<AddArtifactRevisionCommand, AddArtifactRevisionResponse>
    {
        public async Task<DomainResult<AddArtifactRevisionResponse>> Handle(
            AddArtifactRevisionCommand request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure<AddArtifactRevisionResponse>(
                    WorkspaceApplicationErrors.InvalidId);
            }

            if (request.ArtifactId == Guid.Empty)
            {
                return DomainResult.Failure<AddArtifactRevisionResponse>(
                    ArtifactApplicationErrors.InvalidId);
            }

            DomainResult<ArtifactContent> contentResult = ArtifactContent.Create(request.Content);
            if (contentResult.IsFailure)
            {
                return DomainResult.Failure<AddArtifactRevisionResponse>(contentResult.Error);
            }

            Artifact? artifact = await artifactRepository.GetByIdAsync(
                ArtifactId.Create(request.ArtifactId),
                cancellationToken);
            if (artifact is null)
            {
                return DomainResult.Failure<AddArtifactRevisionResponse>(
                    ArtifactApplicationErrors.NotFound(request.ArtifactId));
            }

            if (artifact.WorkspaceId.Value != request.WorkspaceId)
            {
                return DomainResult.Failure<AddArtifactRevisionResponse>(
                    ArtifactApplicationErrors.NotFoundInWorkspace(
                        request.ArtifactId,
                        request.WorkspaceId));
            }

            DateTimeOffset createdAtUtc = clockService.UtcNow;
            DomainResult<ArtifactRevision> revisionResult = artifact.CreateRevision(
                ArtifactRevisionId.Create(Guid.NewGuid()),
                contentResult.Value,
                createdAtUtc);
            if (revisionResult.IsFailure)
            {
                return DomainResult.Failure<AddArtifactRevisionResponse>(revisionResult.Error);
            }

            ArtifactRevision revision = revisionResult.Value;
            DomainResult<ArtifactRuleSet> ruleSetResult = ArtifactRuleFactory.Create(
                artifact,
                revision,
                request.InstructionRules,
                request.PolicyRules);
            if (ruleSetResult.IsFailure)
            {
                return DomainResult.Failure<AddArtifactRevisionResponse>(ruleSetResult.Error);
            }

            await artifactRevisionRepository.AddAsync(revision, cancellationToken);
            await instructionRuleRepository.AddRangeAsync(
                ruleSetResult.Value.InstructionRules,
                cancellationToken);
            await policyRuleRepository.AddRangeAsync(
                ruleSetResult.Value.PolicyRules,
                cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            AddArtifactRevisionResponse response = mapper.Map<AddArtifactRevisionResponse>(
                new ArtifactRevisionResponseMappingSource(artifact, revision));

            return DomainResult.Success(response);
        }
    }
}