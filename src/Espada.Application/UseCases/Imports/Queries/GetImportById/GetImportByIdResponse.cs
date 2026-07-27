namespace Espada.Application.UseCases.Imports.Queries.GetImportById;

public sealed record GetImportByIdResponse(
    Guid Id,
    Guid SourceId,
    Guid WorkspaceId,
    int StatusId,
    string StatusName,
    string Stage,
    int Attempt,
    string? QueueStatus,
    string? FailureCategory,
    bool IsTerminal,
    DateTimeOffset RequestedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    Guid? ArtifactId,
    Guid? ArtifactRevisionId,
    string? FailureCode,
    string? FailureReason);