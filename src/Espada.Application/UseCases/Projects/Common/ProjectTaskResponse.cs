namespace Espada.Application.UseCases.Projects.Common
{
    public sealed record ProjectTaskResponse(
        Guid Id,
        Guid WorkspaceId,
        Guid ProjectId,
        string Title,
        int StatusTypeId,
        string StatusTypeName,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc,
        DateTimeOffset? CompletedAtUtc,
        DateTimeOffset? ArchivedAtUtc);
}