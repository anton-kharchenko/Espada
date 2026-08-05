using Espada.Domain.Rules;
using Espada.LocalSetup.Contracts.Requests;
using Espada.LocalSetup.Contracts.Responses;

namespace Espada.LocalSetup.Contracts
{
    public interface ILocalSetupService
    {
        Task<LocalSetupPreviewResponse> PreviewAsync(
            string path,
            CancellationToken cancellationToken);

        Task<DomainResult<LocalSetupCommitResponse>> CommitAsync(
            CommitLocalSetupRequest request,
            string repositoryPath,
            string issuer,
            string subject,
            CancellationToken cancellationToken);
    }
}