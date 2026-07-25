namespace Espada.Application.UseCases.Workspaces.Common
{
    public sealed record WorkspaceResponse(
        Guid Id,
        string Name,
        int TypeId,
        string TypeName,
        int StatusId,
        string StatusName,
        DateTimeOffset CreatedAtUtc);
}