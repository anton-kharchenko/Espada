namespace Espada.Application.UseCases.Imports.Queries.GetImportById
{
    public sealed record GetImportByIdResponse(
        Guid Id,
        Guid SourceId,
        Guid WorkspaceId,
        int StatusId,
        string StatusName,
        DateTimeOffset RequestedAtUtc,
        DateTimeOffset? StartedAtUtc,
        DateTimeOffset? CompletedAtUtc,
        Guid? ArtifactId,
        Guid? ArtifactRevisionId,
        string? FailureCode,
        string? FailureReason);
}