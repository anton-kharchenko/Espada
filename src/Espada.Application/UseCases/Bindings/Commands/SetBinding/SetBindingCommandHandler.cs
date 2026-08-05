using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.UseCases.Bindings.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Bindings.Commands.SetBinding
{
    internal sealed class SetBindingCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IArtifactRepository artifactRepository,
        IArtifactRevisionRepository artifactRevisionRepository,
        IOrganizationRepository organizationRepository,
        IProjectRepository projectRepository,
        IProjectTaskRepository projectTaskRepository,
        IBindingRepository bindingRepository,
        IUnitOfWork unitOfWork,
        IClockService clockService,
        IMapper mapper)
        : ICommandHandler<SetBindingCommand, BindingResponse>
    {
        public async Task<DomainResult<BindingResponse>> Handle(
            SetBindingCommand request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty || request.ArtifactId == Guid.Empty)
            {
                DomainError error = request.WorkspaceId == Guid.Empty
                    ? WorkspaceApplicationErrors.InvalidId
                    : ArtifactApplicationErrors.InvalidId;
                return DomainResult.Failure<BindingResponse>(error);
            }

            if (request.BindingId == Guid.Empty)
            {
                return DomainResult.Failure<BindingResponse>(BindingApplicationErrors.InvalidId);
            }

            BindingId? requestedBindingId = null;
            if (request.BindingId.HasValue)
            {
                requestedBindingId = BindingId.Create(request.BindingId.Value);
                Binding? existingBinding = await bindingRepository.GetByIdAsync(
                    requestedBindingId,
                    cancellationToken);
                if (existingBinding is not null &&
                    existingBinding.WorkspaceId.Value != request.WorkspaceId)
                {
                    return DomainResult.Failure<BindingResponse>(
                        BindingApplicationErrors.NotFoundInWorkspace(
                            request.BindingId.Value,
                            request.WorkspaceId));
                }
            }

            if (request.OrganizationId == Guid.Empty)
            {
                return DomainResult.Failure<BindingResponse>(OrganizationApplicationErrors.InvalidId);
            }

            if (request.ProjectId == Guid.Empty)
            {
                return DomainResult.Failure<BindingResponse>(ProjectApplicationErrors.InvalidId);
            }

            if (request.TaskId == Guid.Empty)
            {
                return DomainResult.Failure<BindingResponse>(ProjectTaskApplicationErrors.InvalidId);
            }

            Workspace? workspace = await workspaceRepository.GetByIdAsync(
                WorkspaceId.Create(request.WorkspaceId),
                cancellationToken);
            if (workspace is null)
            {
                return DomainResult.Failure<BindingResponse>(
                    WorkspaceApplicationErrors.NotFound(request.WorkspaceId));
            }

            Artifact? artifact = await artifactRepository.GetByIdAsync(
                ArtifactId.Create(request.ArtifactId),
                cancellationToken);
            if (artifact is null || artifact.WorkspaceId.Value != request.WorkspaceId)
            {
                DomainError error = artifact is null
                    ? ArtifactApplicationErrors.NotFound(request.ArtifactId)
                    : ArtifactApplicationErrors.NotFoundInWorkspace(
                        request.ArtifactId,
                        request.WorkspaceId);
                return DomainResult.Failure<BindingResponse>(error);
            }

            if (artifact.CurrentRevisionId is null)
            {
                return DomainResult.Failure<BindingResponse>(
                    ArtifactApplicationErrors.NotFound(request.ArtifactId));
            }

            ArtifactRevision? revision = await artifactRevisionRepository.GetByIdAsync(
                artifact.CurrentRevisionId,
                cancellationToken);
            if (revision is null)
            {
                return DomainResult.Failure<BindingResponse>(
                    ArtifactApplicationErrors.NotFound(request.ArtifactId));
            }

            OrganizationId? organizationId = null;
            if (request.OrganizationId.HasValue)
            {
                Organization? organization = await organizationRepository.GetByIdAsync(
                    OrganizationId.Create(request.OrganizationId.Value),
                    cancellationToken);
                if (organization is null)
                {
                    return DomainResult.Failure<BindingResponse>(
                        OrganizationApplicationErrors.NotFound(request.OrganizationId.Value));
                }

                organizationId = organization.Id;
            }

            Project? project = null;
            if (request.ProjectId.HasValue)
            {
                project = await projectRepository.GetByIdAsync(
                    ProjectId.Create(request.ProjectId.Value),
                    cancellationToken);
                if (project is null || project.WorkspaceId.Value != request.WorkspaceId)
                {
                    DomainError error = project is null
                        ? ProjectApplicationErrors.NotFound(request.ProjectId.Value)
                        : ProjectApplicationErrors.NotFoundInWorkspace(
                            request.ProjectId.Value,
                            request.WorkspaceId);
                    return DomainResult.Failure<BindingResponse>(error);
                }
            }

            ProjectTask? task = null;
            if (request.TaskId.HasValue)
            {
                task = await projectTaskRepository.GetByIdAsync(
                    TaskId.Create(request.TaskId.Value),
                    cancellationToken);
                if (task is null || task.WorkspaceId.Value != request.WorkspaceId)
                {
                    DomainError error = task is null
                        ? ProjectTaskApplicationErrors.NotFound(request.TaskId.Value)
                        : ProjectTaskApplicationErrors.NotFoundInWorkspace(
                            request.TaskId.Value,
                            request.WorkspaceId);
                    return DomainResult.Failure<BindingResponse>(error);
                }
            }

            BindingId bindingId = requestedBindingId ?? BindingId.Create(Guid.NewGuid());
            DomainResult<Binding> bindingResult = artifact.CreateBinding(
                bindingId,
                revision,
                workspace,
                organizationId,
                project,
                request.RepositoryCanonicalUri,
                request.RepositoryRelativePathPrefix,
                request.Branch,
                task,
                request.Agent,
                clockService.UtcNow);
            if (bindingResult.IsFailure)
            {
                return DomainResult.Failure<BindingResponse>(bindingResult.Error);
            }

            await bindingRepository.UpsertAsync(bindingResult.Value, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return DomainResult.Success(mapper.Map<BindingResponse>(bindingResult.Value));
        }
    }
}