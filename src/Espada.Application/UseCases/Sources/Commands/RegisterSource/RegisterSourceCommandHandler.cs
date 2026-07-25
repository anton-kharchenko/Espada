using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Sources.Commands.RegisterSource;

internal sealed class RegisterSourceCommandHandler(IWorkspaceRepository workspaceRepository, ISourceRepository sourceRepository, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<RegisterSourceCommand, RegisterSourceResponse>
{
    public async Task<DomainResult<RegisterSourceResponse>> Handle(RegisterSourceCommand request, CancellationToken cancellationToken)
    {
        if (request.WorkspaceId == Guid.Empty)
        {
            return DomainResult.Failure<RegisterSourceResponse>(WorkspaceApplicationErrors.InvalidId);
        }

        WorkspaceId workspaceId = WorkspaceId.Create(request.WorkspaceId);
        Workspace? workspace = await workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);

        if (workspace is null)
        {
            return DomainResult.Failure<RegisterSourceResponse>(WorkspaceApplicationErrors.NotFound(request.WorkspaceId));
        }

        DomainResult<SourceName> nameResult = SourceName.Create(request.Name);
        
        if (nameResult.IsFailure)
        {
            return DomainResult.Failure<RegisterSourceResponse>(nameResult.Error);
        }

        DomainResult<SourceLocator> locatorResult = SourceLocator.Create(request.Locator);

        if (locatorResult.IsFailure)
        {
            return DomainResult.Failure<RegisterSourceResponse>(locatorResult.Error);
        }

        DomainResult<Source> sourceResult = Source.Create(SourceId.New(), workspaceId, nameResult.Value,  request.Type, locatorResult.Value, clock.UtcNow);

        if (sourceResult.IsFailure)
        {
            return DomainResult.Failure<RegisterSourceResponse>(sourceResult.Error);
        }

        Source source = sourceResult.Value;

        await sourceRepository.AddAsync(source, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        RegisterSourceResponse response = new(source.Id.Value);

        return DomainResult.Success(response);
    }
}