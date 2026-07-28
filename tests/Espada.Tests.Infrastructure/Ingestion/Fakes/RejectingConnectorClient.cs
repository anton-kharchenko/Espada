using Espada.Application.Contracts.Ingestion;
using Espada.Domain.ValueObjects.SourceDefinitions;

namespace Espada.Tests.Infrastructure.Ingestion.Fakes
{
    internal sealed class RejectingConnectorClient : IConnectorSourceClient
    {
        public Task<string> ReadAsync(
            ConnectorSourceDefinition definition,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}