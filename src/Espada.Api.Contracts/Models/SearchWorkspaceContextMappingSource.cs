using Espada.Api.Contracts.Requests.Context;

namespace Espada.Api.Contracts.Models
{
    public sealed record SearchWorkspaceContextMappingSource(Guid WorkspaceId, SearchWorkspaceContextRequest Request);
}