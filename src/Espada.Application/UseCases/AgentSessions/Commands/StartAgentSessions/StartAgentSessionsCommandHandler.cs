using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Agents;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.Models.Agents;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.AgentSessions.Commands.StartAgentSessions
{
    internal sealed class StartAgentSessionsCommandHandler(
        IProjectRepository projectRepository,
        IAgentProfileRepository agentProfileRepository,
        IAgentInstallationRepository agentInstallationRepository,
        IDeviceRepository deviceRepository,
        IAgentSessionRepository agentSessionRepository,
        IAgentWorktreeService worktreeService,
        IAgentSessionExecutionQueue executionQueue,
        IClockService clockService,
        IUnitOfWork unitOfWork) : ICommandHandler<StartAgentSessionsCommand, StartAgentSessionsResponse>
    {
        public async Task<DomainResult<StartAgentSessionsResponse>> Handle(StartAgentSessionsCommand request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty || request.ProjectId == Guid.Empty || request.DeviceId == Guid.Empty
                || string.IsNullOrWhiteSpace(request.Prompt) || request.AgentProfileIds.Count == 0
                || request.AgentProfileIds.Any(profileId => profileId == Guid.Empty))
            {
                return DomainResult.Failure<StartAgentSessionsResponse>(
                    AgentSessionApplicationErrors.InvalidRequest);
            }

            WorkspaceId workspaceId = WorkspaceId.Create(request.WorkspaceId);
            ProjectId projectId = ProjectId.Create(request.ProjectId);
            DeviceId deviceId = DeviceId.Create(request.DeviceId);
            Project? project = await projectRepository.GetByIdAsync(projectId, cancellationToken);
            Device? device = await deviceRepository.GetByIdAsync(deviceId, cancellationToken);
            if (project is null || project.WorkspaceId != workspaceId || device is null)
            {
                return DomainResult.Failure<StartAgentSessionsResponse>(
                    AgentSessionApplicationErrors.InvalidRequest);
            }

            List<(AgentSession Session, AgentSessionExecution Execution)> pending = [];
            foreach (Guid profileIdValue in request.AgentProfileIds.Distinct())
            {
                AgentProfile? profile = await agentProfileRepository.GetByIdAsync(
                    AgentProfileId.Create(profileIdValue), cancellationToken);
                if (profile is null || profile.WorkspaceId != workspaceId)
                {
                    return DomainResult.Failure<StartAgentSessionsResponse>(
                        AgentSessionApplicationErrors.ProfileNotFound(profileIdValue));
                }

                AgentInstallation? installation =
                    await agentInstallationRepository.GetByDeviceAndVendorAsync(deviceId, profile.Vendor.Id,
                        cancellationToken);
                if (installation is null || !installation.IsAuthenticated)
                {
                    return DomainResult.Failure<StartAgentSessionsResponse>(
                        AgentSessionApplicationErrors.InstallationUnavailable(profile.Vendor.Id));
                }

                AgentSessionId sessionId = AgentSessionId.New();
                DomainResult<AgentWorktree> worktree = await worktreeService.PrepareAsync(project, sessionId,
                    profile.Vendor, cancellationToken);
                if (worktree.IsFailure)
                {
                    return DomainResult.Failure<StartAgentSessionsResponse>(worktree.Error);
                }

                DomainResult<AgentSession> sessionResult = AgentSession.Create(sessionId, workspaceId, projectId,
                    profile.Id, deviceId, request.Prompt, worktree.Value.BranchName, worktree.Value.WorktreePath,
                    clockService.UtcNow);
                if (sessionResult.IsFailure)
                {
                    return DomainResult.Failure<StartAgentSessionsResponse>(sessionResult.Error);
                }

                AgentSession session = sessionResult.Value;
                await agentSessionRepository.AddAsync(session, cancellationToken);
                pending.Add((session, new AgentSessionExecution(session.Id.Value, profile.Vendor.Id,
                    installation.ExecutablePath, session.WorktreePath, session.Prompt)));
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            foreach ((AgentSession _, AgentSessionExecution execution) in pending)
            {
                await executionQueue.QueueAsync(execution, cancellationToken);
            }

            return DomainResult.Success(new StartAgentSessionsResponse(
                pending.Select(item => item.Session.Id.Value).ToArray()));
        }
    }
}