using Espada.Application.Models;

namespace Espada.Application.Contracts.Repositories
{
    public interface IRepositoryWatchRegistrationStore
    {
        Task<IReadOnlyList<RepositoryWatchRegistration>> ListAsync(
            CancellationToken cancellationToken = default);
    }
}