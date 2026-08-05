using Espada.Application.Models;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects.SourceDefinitions;

namespace Espada.Application.Contracts.Repositories
{
    public interface IRepositoryScanner
    {
        Task<DomainResult<RepositoryScanResult>> ScanAsync(
            IReadOnlyList<string> localAliases,
            RepositoryScanPolicy scanPolicy,
            CancellationToken cancellationToken = default);
    }
}