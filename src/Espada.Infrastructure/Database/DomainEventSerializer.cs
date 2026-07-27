using Espada.Domain.Events;
using Espada.Domain.SeedWork;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Models;
using System.Text.Json;

namespace Espada.Infrastructure.Database;

internal static class DomainEventSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static (string Name, int Version, string Payload) Serialize(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            ImportJobRequestedDomainEvent requested => ("imports.requested.v1", 1, JsonSerializer.Serialize(new ImportRequestedPayload(requested.ImportJobId.Value, requested.SourceId.Value, requested.WorkspaceId.Value, requested.RequestedAtUtc), SerializerOptions)),
            ImportStageScheduledDomainEvent scheduled => ("imports.stage-scheduled.v1", 1, JsonSerializer.Serialize(new ImportStageScheduledPayload(scheduled.ImportJobId.Value, scheduled.Stage.Id, scheduled.ScheduledAtUtc), SerializerOptions)),
            _ => ($"domain.{ToKebabCase(domainEvent.GetType().Name)}.v1", 1, JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), SerializerOptions))
        };
    }

    public static IDomainEvent? Deserialize(string name, int version, string payload)
    {
        if (version != 1)
        {
            return null;
        }

        return name switch
        {
            "imports.requested.v1" => DeserializeImportRequested(payload),
            "imports.stage-scheduled.v1" => DeserializeImportStageScheduled(payload),
            _ => null
        };
    }

    private static ImportJobRequestedDomainEvent? DeserializeImportRequested(string payload)
    {
        ImportRequestedPayload? value = JsonSerializer.Deserialize<ImportRequestedPayload>(payload, SerializerOptions);
        return value is null ? null : new ImportJobRequestedDomainEvent(ImportJobId.Create(value.ImportJobId), SourceId.Create(value.SourceId), WorkspaceId.Create(value.WorkspaceId), value.RequestedAtUtc);
    }

    private static ImportStageScheduledDomainEvent? DeserializeImportStageScheduled(string payload)
    {
        ImportStageScheduledPayload? value = JsonSerializer.Deserialize<ImportStageScheduledPayload>(payload, SerializerOptions);
        ImportPipelineStageType? stage = value is null ? null : Enumeration.GetAll<ImportPipelineStageType>().SingleOrDefault(stage => stage.Id == value.StageId);

        return value is null || stage is null ? null : new ImportStageScheduledDomainEvent(ImportJobId.Create(value.ImportJobId), stage, value.ScheduledAtUtc);
    }

    private static string ToKebabCase(string value) => string.Concat(value.Select((character, index) => index > 0 && char.IsUpper(character) ? $"-{char.ToLowerInvariant(character)}" : char.ToLowerInvariant(character).ToString()));
}