using Espada.Api.Contracts.Requests.Imports;

namespace Espada.Api.Contracts.Models
{
    public sealed record RequestImportMappingSource(
        Guid WorkspaceId,
        string IdempotencyKey,
        RequestImportRequest Request);
}