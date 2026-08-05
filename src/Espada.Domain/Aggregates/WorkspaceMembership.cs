using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class WorkspaceMembership : AggregateRoot<WorkspaceMembershipId>
    {
        private WorkspaceMembership()
        {
        }

        private WorkspaceMembership(
            WorkspaceMembershipId id,
            WorkspaceId workspaceId,
            string issuer,
            string subject,
            WorkspaceMembershipRoleType role,
            DateTimeOffset joinedAtUtc)
            : base(id)
        {
            WorkspaceId = workspaceId;
            Issuer = issuer;
            Subject = subject;
            Role = role;
            JoinedAtUtc = joinedAtUtc;
        }

        public WorkspaceId WorkspaceId { get; private set; } = null!;

        public string Issuer { get; private set; } = string.Empty;

        public string Subject { get; private set; } = string.Empty;

        public WorkspaceMembershipRoleType Role { get; private set; } = null!;

        public DateTimeOffset JoinedAtUtc { get; private set; }

        public static WorkspaceMembership CreateOwner(
            WorkspaceMembershipId id,
            WorkspaceId workspaceId,
            string issuer,
            string subject,
            DateTimeOffset joinedAtUtc)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(workspaceId);
            ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
            ArgumentException.ThrowIfNullOrWhiteSpace(subject);

            return new WorkspaceMembership(id, workspaceId, issuer.Trim(), subject.Trim(),
                WorkspaceMembershipRoleType.Owner, joinedAtUtc);
        }
    }
}