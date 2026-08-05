using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class AgentSession : AggregateRoot<AgentSessionId>, IHasConcurrencyVersion
    {
        private AgentSession()
        {
        }

        private AgentSession(AgentSessionId id, WorkspaceId workspaceId, ProjectId projectId,
            AgentProfileId agentProfileId, DeviceId deviceId, string prompt, string branchName, string worktreePath,
            DateTimeOffset createdAtUtc) : base(id)
        {
            WorkspaceId = workspaceId;
            ProjectId = projectId;
            AgentProfileId = agentProfileId;
            DeviceId = deviceId;
            Prompt = prompt;
            BranchName = branchName;
            WorktreePath = worktreePath;
            Status = AgentSessionStatusType.Created;
            CreatedAtUtc = createdAtUtc;
            UpdatedAtUtc = createdAtUtc;
        }

        public WorkspaceId WorkspaceId { get; private set; } = null!;
        public ProjectId ProjectId { get; private set; } = null!;
        public AgentProfileId AgentProfileId { get; private set; } = null!;
        public DeviceId DeviceId { get; private set; } = null!;
        public string Prompt { get; private set; } = string.Empty;
        public string BranchName { get; private set; } = string.Empty;
        public string WorktreePath { get; private set; } = string.Empty;
        public AgentSessionStatusType Status { get; private set; } = null!;
        public DateTimeOffset CreatedAtUtc { get; private set; }
        public DateTimeOffset UpdatedAtUtc { get; private set; }
        public DateTimeOffset? FinishedAtUtc { get; private set; }
        public uint Version { get; private set; }

        public static DomainResult<AgentSession> Create(AgentSessionId id, WorkspaceId workspaceId,
            ProjectId projectId, AgentProfileId agentProfileId, DeviceId deviceId, string? prompt,
            string? branchName, string? worktreePath, DateTimeOffset createdAtUtc)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(workspaceId);
            ArgumentNullException.ThrowIfNull(projectId);
            ArgumentNullException.ThrowIfNull(agentProfileId);
            ArgumentNullException.ThrowIfNull(deviceId);
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return DomainResult<AgentSession>.Failure(AgentSessionErrors.PromptEmpty);
            }

            if (string.IsNullOrWhiteSpace(branchName))
            {
                return DomainResult<AgentSession>.Failure(AgentSessionErrors.BranchNameEmpty);
            }

            if (string.IsNullOrWhiteSpace(worktreePath))
            {
                return DomainResult<AgentSession>.Failure(AgentSessionErrors.WorktreePathEmpty);
            }

            return DomainResult<AgentSession>.Success(new AgentSession(id, workspaceId, projectId, agentProfileId,
                deviceId, prompt.Trim(), branchName.Trim(), worktreePath.Trim(), createdAtUtc));
        }

        public DomainResult Start(DateTimeOffset changedAtUtc)
        {
            return Transition(AgentSessionStatusType.Created, AgentSessionStatusType.Running, changedAtUtc);
        }

        public DomainResult WaitForApproval(DateTimeOffset changedAtUtc)
        {
            return Transition(AgentSessionStatusType.Running, AgentSessionStatusType.WaitingForApproval, changedAtUtc);
        }

        public DomainResult ResumeAfterApproval(DateTimeOffset changedAtUtc)
        {
            return Transition(AgentSessionStatusType.WaitingForApproval, AgentSessionStatusType.Running, changedAtUtc);
        }

        public DomainResult Complete(DateTimeOffset changedAtUtc)
        {
            return Finish(AgentSessionStatusType.Running, AgentSessionStatusType.Completed, changedAtUtc);
        }

        public DomainResult Fail(DateTimeOffset changedAtUtc)
        {
            if (!Status.Equals(AgentSessionStatusType.Running) &&
                !Status.Equals(AgentSessionStatusType.WaitingForApproval))
            {
                return DomainResult.Failure(
                    AgentSessionErrors.InvalidTransition(Status, AgentSessionStatusType.Failed));
            }

            Status = AgentSessionStatusType.Failed;
            UpdatedAtUtc = changedAtUtc;
            FinishedAtUtc = changedAtUtc;
            return DomainResult.Success();
        }

        public DomainResult Cancel(DateTimeOffset changedAtUtc)
        {
            if (Status.Equals(AgentSessionStatusType.Completed) || Status.Equals(AgentSessionStatusType.Failed) ||
                Status.Equals(AgentSessionStatusType.Cancelled))
            {
                return DomainResult.Failure(
                    AgentSessionErrors.InvalidTransition(Status, AgentSessionStatusType.Cancelled));
            }

            Status = AgentSessionStatusType.Cancelled;
            UpdatedAtUtc = changedAtUtc;
            FinishedAtUtc = changedAtUtc;
            return DomainResult.Success();
        }

        private DomainResult Transition(AgentSessionStatusType expected, AgentSessionStatusType next,
            DateTimeOffset changedAtUtc)
        {
            if (!Status.Equals(expected))
            {
                return DomainResult.Failure(AgentSessionErrors.InvalidTransition(Status, next));
            }

            Status = next;
            UpdatedAtUtc = changedAtUtc;
            return DomainResult.Success();
        }

        private DomainResult Finish(AgentSessionStatusType expected, AgentSessionStatusType next,
            DateTimeOffset changedAtUtc)
        {
            DomainResult result = Transition(expected, next, changedAtUtc);
            if (result.IsSuccess)
            {
                FinishedAtUtc = changedAtUtc;
            }

            return result;
        }
    }
}
