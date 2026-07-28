using Espada.Api.Contracts.Requests.Billing;

namespace Espada.Api.Contracts.Models
{
    public sealed record CreateCheckoutMappingSource(
        Guid WorkspaceId,
        CreateCheckoutRequest Request,
        string IdempotencyKey);
}