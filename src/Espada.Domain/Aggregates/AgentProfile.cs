using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class AgentProfile : AggregateRoot<AgentProfileId>, IHasConcurrencyVersion
    {
        public const int NameMaxLength = 200;

        private AgentProfile()
        {
        }

        private AgentProfile(AgentProfileId id, WorkspaceId workspaceId, AgentVendorType vendor, string name,
            string settingsJson, DateTimeOffset createdAtUtc) : base(id)
        {
            WorkspaceId = workspaceId;
            Vendor = vendor;
            Name = name;
            SettingsJson = settingsJson;
            CreatedAtUtc = createdAtUtc;
            UpdatedAtUtc = createdAtUtc;
        }

        public WorkspaceId WorkspaceId { get; private set; } = null!;
        public AgentVendorType Vendor { get; private set; } = null!;
        public string Name { get; private set; } = string.Empty;
        public string SettingsJson { get; private set; } = "{}";
        public DateTimeOffset CreatedAtUtc { get; private set; }
        public DateTimeOffset UpdatedAtUtc { get; private set; }
        public uint Version { get; private set; }

        public static DomainResult<AgentProfile> Create(AgentProfileId id, WorkspaceId workspaceId,
            AgentVendorType vendor, string? name, string? settingsJson, DateTimeOffset createdAtUtc)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(workspaceId);
            ArgumentNullException.ThrowIfNull(vendor);
            if (string.IsNullOrWhiteSpace(name))
            {
                return DomainResult<AgentProfile>.Failure(AgentProfileErrors.NameEmpty);
            }

            if (string.IsNullOrWhiteSpace(settingsJson))
            {
                return DomainResult<AgentProfile>.Failure(AgentProfileErrors.SettingsEmpty);
            }

            string normalizedName = name.Trim();
            return normalizedName.Length > NameMaxLength
                ? DomainResult<AgentProfile>.Failure(AgentProfileErrors.NameTooLong)
                : DomainResult<AgentProfile>.Success(new AgentProfile(id, workspaceId, vendor, normalizedName,
                    settingsJson, createdAtUtc));
        }
    }
}
