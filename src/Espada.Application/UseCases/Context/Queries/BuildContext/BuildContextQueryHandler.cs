using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;
using Espada.Application.Services;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Espada.Application.Constants;

namespace Espada.Application.UseCases.Context.Queries.BuildContext
{
    internal sealed class BuildContextQueryHandler(
        IWorkspaceRepository workspaceRepository,
        IProjectRepository projectRepository,
        IProjectTaskRepository projectTaskRepository,
        IContextCandidateStore candidateStore,
        ContextResolver resolver,
        IMapper mapper)
        : IQueryHandler<BuildContextQuery, BuildContextResponse>
    {
        public async Task<DomainResult<BuildContextResponse>> Handle(
            BuildContextQuery request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure<BuildContextResponse>(
                    WorkspaceApplicationErrors.InvalidId);
            }

            if (request.ProjectId == Guid.Empty)
            {
                return DomainResult.Failure<BuildContextResponse>(
                    ProjectApplicationErrors.InvalidId);
            }

            if (request.TaskId == Guid.Empty)
            {
                return DomainResult.Failure<BuildContextResponse>(
                    ProjectTaskApplicationErrors.InvalidId);
            }

            if (!ContextAgentConstants.IsSupported(request.Agent))
            {
                return DomainResult.Failure<BuildContextResponse>(
                    ContextApplicationErrors.AgentUnsupported);
            }

            if (request.TokenBudget <= 0)
            {
                return DomainResult.Failure<BuildContextResponse>(
                    ContextApplicationErrors.TokenBudgetInvalid);
            }

            bool projectRequired = request.TaskId.HasValue
                                   || !string.IsNullOrWhiteSpace(
                                       request.RepositoryRelativePath)
                                   || !string.IsNullOrWhiteSpace(request.Branch);
            if (projectRequired && !request.ProjectId.HasValue)
            {
                return DomainResult.Failure<BuildContextResponse>(
                    ContextApplicationErrors.ProjectRequired);
            }

            if (!TryNormalizePath(
                    request.RepositoryRelativePath,
                    out string? repositoryRelativePath))
            {
                return DomainResult.Failure<BuildContextResponse>(
                    ContextApplicationErrors.RepositoryRelativePathInvalid);
            }

            string? branch = Normalize(request.Branch);
            string agent = request.Agent.Trim().ToLowerInvariant();
            Workspace? workspace = await workspaceRepository.GetByIdAsync(
                WorkspaceId.Create(request.WorkspaceId),
                cancellationToken);
            if (workspace is null)
            {
                return DomainResult.Failure<BuildContextResponse>(
                    WorkspaceApplicationErrors.NotFound(request.WorkspaceId));
            }

            Project? project = null;
            if (request.ProjectId.HasValue)
            {
                project = await projectRepository.GetByIdAsync(
                    ProjectId.Create(request.ProjectId.Value),
                    cancellationToken);
                if (project is null)
                {
                    return DomainResult.Failure<BuildContextResponse>(
                        ProjectApplicationErrors.NotFound(
                            request.ProjectId.Value));
                }

                if (!project.WorkspaceId.Equals(workspace.Id))
                {
                    return DomainResult.Failure<BuildContextResponse>(
                        ProjectApplicationErrors.NotFoundInWorkspace(
                            request.ProjectId.Value,
                            request.WorkspaceId));
                }
            }

            ProjectTask? task = null;
            if (request.TaskId.HasValue)
            {
                task = await projectTaskRepository.GetByIdAsync(
                    TaskId.Create(request.TaskId.Value),
                    cancellationToken);
                if (task is null)
                {
                    return DomainResult.Failure<BuildContextResponse>(
                        ProjectTaskApplicationErrors.NotFound(
                            request.TaskId.Value));
                }

                if (!task.WorkspaceId.Equals(workspace.Id))
                {
                    return DomainResult.Failure<BuildContextResponse>(
                        ProjectTaskApplicationErrors.NotFoundInWorkspace(
                            request.TaskId.Value,
                            request.WorkspaceId));
                }

                if (!task.ProjectId.Equals(project!.Id))
                {
                    return DomainResult.Failure<BuildContextResponse>(
                        ProjectTaskApplicationErrors.NotFoundInProject(
                            request.TaskId.Value,
                            project.Id.Value));
                }
            }

            IReadOnlyList<ContextCandidateRecord> candidates =
                await candidateStore.LoadByWorkspaceIdAsync(
                    workspace.Id,
                    cancellationToken);
            ContextResolutionRequest resolutionRequest = new(
                workspace,
                project,
                task,
                repositoryRelativePath,
                branch,
                agent,
                request.TokenBudget);
            DomainResult<ResolvedContext> resolution = resolver.Resolve(
                resolutionRequest,
                candidates);

            return resolution.IsFailure
                ? DomainResult.Failure<BuildContextResponse>(resolution.Error)
                : DomainResult.Success(
                    mapper.Map<BuildContextResponse>(resolution.Value));
        }

        private static bool TryNormalizePath(
            string? value,
            out string? normalized)
        {
            normalized = Normalize(value)?.Replace('\\', '/');
            if (normalized is null)
            {
                return true;
            }

            string[] segments = normalized.Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries);
            if (Path.IsPathRooted(normalized)
                || segments.Any(segment => segment is "." or ".."))
            {
                normalized = null;
                return false;
            }

            normalized = string.Join('/', segments);
            return true;
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}