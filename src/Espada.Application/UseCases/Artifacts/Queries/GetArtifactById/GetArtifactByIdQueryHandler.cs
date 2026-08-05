using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Artifacts.Queries.GetArtifactById
{
    internal sealed class GetArtifactByIdQueryHandler(
        IArtifactRepository artifactRepository,
        IArtifactRevisionRepository artifactRevisionRepository,
        IInstructionRuleRepository instructionRuleRepository,
        IPolicyRuleRepository policyRuleRepository,
        IMapper mapper)
        : IQueryHandler<GetArtifactByIdQuery, GetArtifactByIdResponse>
    {
        public async Task<DomainResult<GetArtifactByIdResponse>> Handle(
            GetArtifactByIdQuery request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure<GetArtifactByIdResponse>(
                    WorkspaceApplicationErrors.InvalidId);
            }

            if (request.ArtifactId == Guid.Empty)
            {
                return DomainResult.Failure<GetArtifactByIdResponse>(
                    ArtifactApplicationErrors.InvalidId);
            }

            Artifact? artifact = await artifactRepository.GetByIdAsync(
                ArtifactId.Create(request.ArtifactId),
                cancellationToken);
            if (artifact is null)
            {
                return DomainResult.Failure<GetArtifactByIdResponse>(
                    ArtifactApplicationErrors.NotFound(request.ArtifactId));
            }

            if (artifact.WorkspaceId.Value != request.WorkspaceId)
            {
                return DomainResult.Failure<GetArtifactByIdResponse>(
                    ArtifactApplicationErrors.NotFoundInWorkspace(
                        request.ArtifactId,
                        request.WorkspaceId));
            }

            ArtifactRevision? revision = artifact.CurrentRevisionId is null
                ? null
                : await artifactRevisionRepository.GetByIdAsync(
                    artifact.CurrentRevisionId,
                    cancellationToken);
            IReadOnlyList<InstructionRule> instructionRules = revision is null
                ? []
                : await instructionRuleRepository.ListByRevisionIdAsync(
                    revision.Id,
                    cancellationToken);
            IReadOnlyList<PolicyRule> policyRules = revision is null
                ? []
                : await policyRuleRepository.ListByRevisionIdAsync(
                    revision.Id,
                    cancellationToken);
            GetArtifactByIdMappingSource mappingSource = new(
                artifact,
                revision,
                instructionRules,
                policyRules);
            GetArtifactByIdResponse response = mapper.Map<GetArtifactByIdResponse>(mappingSource);

            return DomainResult.Success(response);
        }
    }
}