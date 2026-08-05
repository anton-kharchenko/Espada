namespace Espada.Application.UseCases.Projects.Common
{
    public sealed record ProjectResponse(
        Guid Id,
        Guid WorkspaceId,
        string Name,
        string CanonicalRemoteUri,
        IReadOnlyList<string> LocalAliases,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);
}