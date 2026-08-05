namespace Espada.Infrastructure.Sync.Contracts
{
    public interface ISyncAuthorizationService
    {
        Uri Begin(Uri redirectUri);

        Task<string?> GetAccessTokenAsync(
            CancellationToken cancellationToken);

        Task CompleteAsync(
            string state,
            string code,
            CancellationToken cancellationToken);
    }
}