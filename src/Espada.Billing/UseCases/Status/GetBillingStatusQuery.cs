using Espada.Application.Contracts.Messaging;
using Espada.Billing.Models;

namespace Espada.Billing.UseCases.Status;

public sealed record GetBillingStatusQuery(Guid WorkspaceId) : IQuery<BillingStatusSnapshot>;