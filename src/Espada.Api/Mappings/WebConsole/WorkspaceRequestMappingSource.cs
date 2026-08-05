namespace Espada.Api.Mappings.WebConsole
{
    internal sealed record WorkspaceRequestMappingSource<TRequest>(
        Guid WorkspaceId,
        TRequest Request);
}