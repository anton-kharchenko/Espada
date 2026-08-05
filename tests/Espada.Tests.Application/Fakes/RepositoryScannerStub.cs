using Espada.Application.Contracts.Repositories;
using Espada.Application.Models;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects.SourceDefinitions;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class RepositoryScannerStub : IRepositoryScanner
    {
        public DomainResult<RepositoryScanResult> ResultToReturn { get; set; } =
            DomainResult.Success(new RepositoryScanResult("C:\\repository", []));

        public Task<DomainResult<RepositoryScanResult>> ScanAsync(IReadOnlyList<string> localAliases,
            RepositoryScanPolicy scanPolicy, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ResultToReturn);
        }
    }
}