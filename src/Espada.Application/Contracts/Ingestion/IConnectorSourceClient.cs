using Espada.Domain.ValueObjects.SourceDefinitions;

namespace Espada.Application.Contracts.Ingestion
{
    public interface IConnectorSourceClient
    {
        Task<string> ReadAsync(ConnectorSourceDefinition definition, CancellationToken cancellationToken = default);
    }
}