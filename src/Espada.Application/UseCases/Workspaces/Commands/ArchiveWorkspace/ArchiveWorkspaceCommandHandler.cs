using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Workspaces.Commands.ArchiveWorkspace
{
    internal sealed class ArchiveWorkspaceCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork,
        IClockService clockService) : ICommandHandler<ArchiveWorkspaceCommand>
    {
        public async Task<DomainResult> Handle(ArchiveWorkspaceCommand request, CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure(WorkspaceApplicationErrors.InvalidId);
            }

            WorkspaceId workspaceId = WorkspaceId.Create(request.WorkspaceId);

            Workspace? workspace = await workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);

            if (workspace is null)
            {
                return DomainResult.Failure(WorkspaceApplicationErrors.NotFound(request.WorkspaceId));
            }

            DomainResult archiveResult = workspace.Archive(clockService.UtcNow);

            if (archiveResult.IsFailure)
            {
                return archiveResult;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return DomainResult.Success();
        }
    }
}