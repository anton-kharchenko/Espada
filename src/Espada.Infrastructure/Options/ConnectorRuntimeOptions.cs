namespace Espada.Infrastructure.Options;

public sealed class ConnectorRuntimeOptions
{
    public List<ApprovedConnectorOptions> Approved { get; set; } = [];
}