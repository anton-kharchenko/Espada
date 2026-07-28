using Espada.Api.Contracts.Requests.Sources;

namespace Espada.Api.Contracts.Models
{
    public sealed record RegisterSourceMappingSource(Guid WorkspaceId, RegisterSourceRequest Request);
}