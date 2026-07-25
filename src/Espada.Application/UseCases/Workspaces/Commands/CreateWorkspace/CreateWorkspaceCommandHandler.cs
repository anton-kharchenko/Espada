using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;

internal sealed class CreateWorkspaceCommandHandler(IWorkspaceRepository workspaceRepository, IUnitOfWork unitOfWork, IClock clock) : ICommandHandler<CreateWorkspaceCommand, CreateWorkspaceResponse>
{
    public async Task<DomainResult<CreateWorkspaceResponse>> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        DomainResult<WorkspaceName> nameResult = WorkspaceName.Create(request.Name);
        if (nameResult.IsFailure)
        {
            return DomainResult.Failure<CreateWorkspaceResponse>(nameResult.Error);
        }

        WorkspaceId workspaceId = WorkspaceId.New();
        DomainResult<Workspace> workspaceResult = Workspace.Create(workspaceId, nameResult.Value, request.Type, clock.UtcNow);
        if (workspaceResult.IsFailure)
        {
            return DomainResult.Failure<CreateWorkspaceResponse>(workspaceResult.Error);
        }

        Workspace workspace = workspaceResult.Value;

        await workspaceRepository.AddAsync(workspace, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        CreateWorkspaceResponse response = new(workspace.Id.Value);

        return DomainResult.Success(response);
    }
}