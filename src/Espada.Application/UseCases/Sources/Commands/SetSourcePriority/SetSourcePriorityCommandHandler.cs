using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Sources.Commands.SetSourcePriority;

internal sealed class SetSourcePriorityCommandHandler(ISourceRepository sourceRepository, IUnitOfWork unitOfWork, IClockService clockService) : ICommandHandler<SetSourcePriorityCommand>
{
    public async Task<DomainResult> Handle(SetSourcePriorityCommand request, CancellationToken cancellationToken)
    {
        if (request.WorkspaceId == Guid.Empty)
        {
            return DomainResult.Failure(WorkspaceApplicationErrors.InvalidId);
        }

        if (request.SourceId == Guid.Empty)
        {
            return DomainResult.Failure(SourceApplicationErrors.InvalidId);
        }

        Source? source = await sourceRepository.GetByIdAsync(SourceId.Create(request.SourceId), cancellationToken);
        if (source is null)
        {
            return DomainResult.Failure(SourceApplicationErrors.NotFound(request.SourceId));
        }

        if (source.WorkspaceId.Value != request.WorkspaceId)
        {
            return DomainResult.Failure(SourceApplicationErrors.NotFoundInWorkspace(request.SourceId, request.WorkspaceId));
        }

        DomainResult<ContextPriority> priorityResult = ContextPriority.Create(request.Priority);
        if (priorityResult.IsFailure)
        {
            return DomainResult.Failure(priorityResult.Error);
        }

        DomainResult result = source.SetPriority(priorityResult.Value, clockService.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return DomainResult.Success();
    }
}