namespace Espada.Api.WebConsole.Mappings
{
    internal sealed record WorkspaceRequestMappingSource<TRequest>(
        Guid WorkspaceId,
        TRequest Request);
}