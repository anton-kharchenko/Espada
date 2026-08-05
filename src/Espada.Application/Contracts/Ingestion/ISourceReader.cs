using Espada.Application.Models;
using Espada.Domain.ValueObjects.SourceDefinitions;

namespace Espada.Application.Contracts.Ingestion
{
    public interface ISourceReader
    {
        Task<SourceReadResult> ReadAsync(
            SourceDefinition definition,
            RepositoryFileImportOptions? repositoryFile = null,
            CancellationToken cancellationToken = default);
    }
}