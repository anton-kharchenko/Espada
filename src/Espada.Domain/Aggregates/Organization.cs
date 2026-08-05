using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class Organization : AggregateRoot<OrganizationId>, IHasConcurrencyVersion
    {
        public const int NameMaxLength = 200;

        private Organization()
        {
        }

        private Organization(OrganizationId id, string name, DateTimeOffset createdAtUtc) : base(id)
        {
            Name = name;
            CreatedAtUtc = createdAtUtc;
        }

        public string Name { get; private set; } = string.Empty;
        public DateTimeOffset CreatedAtUtc { get; private set; }
        public uint Version { get; private set; }

        public static DomainResult<Organization> Create(OrganizationId id, string? name, DateTimeOffset createdAtUtc)
        {
            ArgumentNullException.ThrowIfNull(id);
            if (string.IsNullOrWhiteSpace(name))
            {
                return DomainResult<Organization>.Failure(OrganizationErrors.NameEmpty);
            }

            string normalizedName = name.Trim();
            return normalizedName.Length > NameMaxLength
                ? DomainResult<Organization>.Failure(OrganizationErrors.NameTooLong)
                : DomainResult<Organization>.Success(new Organization(id, normalizedName, createdAtUtc));
        }

        public DomainResult<OrganizationMembership> CreateMembership(OrganizationMembershipId id, string? issuer,
            string? subject, OrganizationMembershipRoleType role, DateTimeOffset joinedAtUtc)
        {
            return OrganizationMembership.Create(id, this, issuer, subject, role, joinedAtUtc);
        }
    }
}