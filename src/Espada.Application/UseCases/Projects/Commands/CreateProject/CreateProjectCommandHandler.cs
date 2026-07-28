using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.UseCases.Projects.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Projects.Commands.CreateProject
{
    internal sealed class CreateProjectCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork,
        IClockService clockService,
        IMapper mapper)
        : ICommandHandler<CreateProjectCommand, ProjectResponse>
    {
        public async Task<DomainResult<ProjectResponse>> Handle(
            CreateProjectCommand request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure<ProjectResponse>(WorkspaceApplicationErrors.InvalidId);
            }

            WorkspaceId workspaceId = WorkspaceId.Create(request.WorkspaceId);
            Workspace? workspace = await workspaceRepository.GetByIdAsync(
                workspaceId,
                cancellationToken);
            if (workspace is null)
            {
                return DomainResult.Failure<ProjectResponse>(
                    WorkspaceApplicationErrors.NotFound(request.WorkspaceId));
            }

            string normalizedRemoteUri = request.CanonicalRemoteUri?.Trim() ?? string.Empty;
            if (normalizedRemoteUri.Length > 0
                && await projectRepository.ExistsByCanonicalRemoteUriAsync(
                    workspaceId,
                    normalizedRemoteUri,
                    cancellationToken))
            {
                return DomainResult.Failure<ProjectResponse>(
                    ProjectApplicationErrors.DuplicateCanonicalRemoteUri(normalizedRemoteUri));
            }

            DomainResult<Project> projectResult = Project.Create(
                ProjectId.Create(Guid.NewGuid()),
                workspaceId,
                request.Name,
                request.CanonicalRemoteUri,
                request.LocalAliases,
                clockService.UtcNow);
            if (projectResult.IsFailure)
            {
                return DomainResult.Failure<ProjectResponse>(projectResult.Error);
            }

            await projectRepository.AddAsync(projectResult.Value, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return DomainResult.Success(mapper.Map<ProjectResponse>(projectResult.Value));
        }
    }
}