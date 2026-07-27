namespace Espada.Application.Contracts.Billing;

public interface IImportAdmissionPolicy
{
    Task<string?> GetDenialReasonAsync(Guid workspaceId, CancellationToken cancellationToken = default);
}