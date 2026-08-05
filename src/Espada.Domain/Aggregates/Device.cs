using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class Device : AggregateRoot<DeviceId>, IHasConcurrencyVersion
    {
        public const int NameMaxLength = 200;

        private Device()
        {
        }

        private Device(DeviceId id, string name, DateTimeOffset createdAtUtc) : base(id)
        {
            Name = name;
            CreatedAtUtc = createdAtUtc;
            LastSeenAtUtc = createdAtUtc;
        }

        public string Name { get; private set; } = string.Empty;
        public DateTimeOffset CreatedAtUtc { get; private set; }
        public DateTimeOffset LastSeenAtUtc { get; private set; }
        public uint Version { get; private set; }

        public static DomainResult<Device> Create(DeviceId id, string? name, DateTimeOffset createdAtUtc)
        {
            ArgumentNullException.ThrowIfNull(id);
            if (string.IsNullOrWhiteSpace(name))
            {
                return DomainResult<Device>.Failure(DeviceErrors.NameEmpty);
            }

            string normalizedName = name.Trim();
            return normalizedName.Length > NameMaxLength
                ? DomainResult<Device>.Failure(DeviceErrors.NameTooLong)
                : DomainResult<Device>.Success(new Device(id, normalizedName, createdAtUtc));
        }

        public void MarkSeen(DateTimeOffset seenAtUtc)
        {
            if (seenAtUtc > LastSeenAtUtc)
            {
                LastSeenAtUtc = seenAtUtc;
            }
        }
    }
}