using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Artifacts.Commands.SetArtifactPriority;

internal sealed class SetArtifactPriorityCommandHandler(IArtifactRepository artifactRepository, IUnitOfWork unitOfWork, IClockService clockService) : ICommandHandler<SetArtifactPriorityCommand>
{
    public async Task<DomainResult> Handle(SetArtifactPriorityCommand request, CancellationToken cancellationToken)
    {
        if (request.WorkspaceId == Guid.Empty)
        {
            return DomainResult.Failure(WorkspaceApplicationErrors.InvalidId);
        }

        if (request.ArtifactId == Guid.Empty)
        {
            return DomainResult.Failure(ArtifactApplicationErrors.InvalidId);
        }

        Artifact? artifact = await artifactRepository.GetByIdAsync(ArtifactId.Create(request.ArtifactId), cancellationToken);

        if (artifact is null)
        {
            return DomainResult.Failure(ArtifactApplicationErrors.NotFound(request.ArtifactId));
        }

        if (artifact.WorkspaceId.Value != request.WorkspaceId)
        {
            return DomainResult.Failure(ArtifactApplicationErrors.NotFoundInWorkspace(request.ArtifactId, request.WorkspaceId));
        }

        DomainResult<ContextPriority> priorityResult = ContextPriority.Create(request.Priority);

        if (priorityResult.IsFailure)
        {
            return DomainResult.Failure(priorityResult.Error);
        }

        DomainResult result = artifact.SetPriority(priorityResult.Value, clockService.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return DomainResult.Success();
    }
}