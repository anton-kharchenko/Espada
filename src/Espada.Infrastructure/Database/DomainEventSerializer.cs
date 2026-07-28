using Espada.Domain.Constants;
using Espada.Domain.Enums;
using Espada.Domain.Events;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Models;
using System.Text.Json;

namespace Espada.Infrastructure.Database
{
    internal static class DomainEventSerializer
    {
        private static readonly JsonSerializerOptions SerializerOptions =
            new(JsonSerializerDefaults.Web);

        public static (string Name, int Version, string Payload) Serialize(IDomainEvent domainEvent)
        {
            return domainEvent switch
            {
                ImportJobRequestedDomainEvent requested =>
                (
                    DomainEventContractConstants.ImportRequested,
                    DomainEventContractConstants.CurrentVersion,
                    JsonSerializer.Serialize(
                        new ImportRequestedPayload(requested.ImportJobId.Value, requested.SourceId.Value,
                            requested.WorkspaceId.Value, requested.RequestedAtUtc), SerializerOptions)
                ),
                ImportStageScheduledDomainEvent scheduled =>
                (
                    DomainEventContractConstants.ImportStageScheduled,
                    DomainEventContractConstants.CurrentVersion,
                    JsonSerializer.Serialize(
                        new ImportStageScheduledPayload(scheduled.ImportJobId.Value, scheduled.Stage.Id,
                            scheduled.ScheduledAtUtc), SerializerOptions)
                ),
                _ =>
                (
                    $"domain.{ToKebabCase(domainEvent.GetType().Name)}.v{DomainEventContractConstants.CurrentVersion}",
                    DomainEventContractConstants.CurrentVersion,
                    JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), SerializerOptions)
                )
            };
        }

        public static IDomainEvent? Deserialize(string name, int version, string payload)
        {
            if (version != DomainEventContractConstants.CurrentVersion)
            {
                return null;
            }

            return name switch
            {
                DomainEventContractConstants.ImportRequested => DeserializeImportRequested(payload),
                DomainEventContractConstants.ImportStageScheduled => DeserializeImportStageScheduled(payload),
                _ => null
            };
        }

        private static ImportJobRequestedDomainEvent? DeserializeImportRequested(string payload)
        {
            ImportRequestedPayload? value =
                JsonSerializer.Deserialize<ImportRequestedPayload>(payload, SerializerOptions);

            return value is null
                ? null
                : new ImportJobRequestedDomainEvent(
                    ImportJobId.Create(value.ImportJobId),
                    SourceId.Create(value.SourceId),
                    WorkspaceId.Create(value.WorkspaceId),
                    value.RequestedAtUtc);
        }

        private static ImportStageScheduledDomainEvent? DeserializeImportStageScheduled(string payload)
        {
            ImportStageScheduledPayload? value =
                JsonSerializer.Deserialize<ImportStageScheduledPayload>(payload, SerializerOptions);
            if (value is null)
            {
                return null;
            }

            return new ImportStageScheduledDomainEvent(ImportJobId.Create(value.ImportJobId),
                Enumeration.FromId<ImportPipelineStageType>(value.StageId), value.ScheduledAtUtc);
        }

        private static string ToKebabCase(string value)
        {
            return string.Concat(value.Select((character, index) =>
                index > 0 && char.IsUpper(character)
                    ? $"-{char.ToLowerInvariant(character)}"
                    : char.ToLowerInvariant(character).ToString()));
        }
    }
}