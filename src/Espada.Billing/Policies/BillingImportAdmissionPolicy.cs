using Espada.Application.Contracts.Billing;
using Espada.Billing.Contracts;
using Espada.Billing.Enums;
using Espada.Billing.Models;

namespace Espada.Billing.Policies
{
    internal sealed class BillingImportAdmissionPolicy(IBillingStoreService storeService) : IImportAdmissionPolicy
    {
        public async Task<string?> GetDenialReasonAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        {
            BillingCustomerSnapshot? customer =
                await storeService.GetCustomerByWorkspaceAsync(workspaceId, cancellationToken);
            BillingAccessStateType stateType =
                customer?.GetAccessState(DateTimeOffset.UtcNow) ?? BillingAccessStateType.ReadOnly;
            return stateType is BillingAccessStateType.ReadOnly or BillingAccessStateType.SyncDisabled
                ? "New cloud imports are blocked until billing is recovered."
                : null;
        }
    }
}