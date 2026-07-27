namespace Espada.Application.UseCases.Sources.Common
{
    public sealed record SourceResponse(
        Guid Id,
        Guid WorkspaceId,
        string Name,
        string Locator,
        int TypeId,
        string TypeName,
        int StatusId,
        string StatusName,
        int Priority,
        DateTimeOffset CreatedAtUtc);
}