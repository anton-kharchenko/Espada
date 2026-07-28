using Espada.Application.Contracts.Billing;

namespace Espada.Application.Policies.Billing
{
    internal sealed class AllowImportAdmissionPolicy : IImportAdmissionPolicy
    {
        public Task<string?> GetDenialReasonAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<string?>(null);
        }
    }
}