using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class OrganizationMembership : AggregateRoot<OrganizationMembershipId>
    {
        public const int IssuerMaxLength = 500;
        public const int SubjectMaxLength = 200;

        private OrganizationMembership()
        {
        }

        private OrganizationMembership(OrganizationMembershipId id, OrganizationId organizationId, string issuer,
            string subject, OrganizationMembershipRoleType role, DateTimeOffset joinedAtUtc) : base(id)
        {
            OrganizationId = organizationId;
            Issuer = issuer;
            Subject = subject;
            Role = role;
            JoinedAtUtc = joinedAtUtc;
        }

        public OrganizationId OrganizationId { get; private set; } = null!;
        public string Issuer { get; private set; } = string.Empty;
        public string Subject { get; private set; } = string.Empty;
        public OrganizationMembershipRoleType Role { get; private set; } = null!;
        public DateTimeOffset JoinedAtUtc { get; private set; }

        internal static DomainResult<OrganizationMembership> Create(OrganizationMembershipId id,
            Organization organization, string? issuer, string? subject, OrganizationMembershipRoleType role,
            DateTimeOffset joinedAtUtc)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(organization);
            ArgumentNullException.ThrowIfNull(role);
            if (string.IsNullOrWhiteSpace(issuer))
            {
                return DomainResult<OrganizationMembership>.Failure(OrganizationMembershipErrors.IssuerEmpty);
            }

            if (string.IsNullOrWhiteSpace(subject))
            {
                return DomainResult<OrganizationMembership>.Failure(OrganizationMembershipErrors.SubjectEmpty);
            }

            string normalizedIssuer = issuer.Trim();
            string normalizedSubject = subject.Trim();
            if (normalizedIssuer.Length > IssuerMaxLength)
            {
                return DomainResult<OrganizationMembership>.Failure(OrganizationMembershipErrors.IssuerTooLong);
            }

            if (normalizedSubject.Length > SubjectMaxLength)
            {
                return DomainResult<OrganizationMembership>.Failure(OrganizationMembershipErrors.SubjectTooLong);
            }

            return DomainResult<OrganizationMembership>.Success(new OrganizationMembership(id, organization.Id,
                normalizedIssuer, normalizedSubject, role, joinedAtUtc));
        }
    }
}