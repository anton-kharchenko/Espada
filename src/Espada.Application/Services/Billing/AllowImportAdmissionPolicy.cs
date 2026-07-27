using Espada.Application.Contracts.Billing;

namespace Espada.Application.Services.Billing;

internal sealed class AllowImportAdmissionPolicy : IImportAdmissionPolicy
{
    public Task<string?> GetDenialReasonAsync(Guid workspaceId, CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(null);
}