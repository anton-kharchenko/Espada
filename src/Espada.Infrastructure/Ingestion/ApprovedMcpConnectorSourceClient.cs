using Espada.Application.Contracts.Ingestion;
using Espada.Application.Enums;
using Espada.Application.Exceptions;
using Espada.Domain.ValueObjects.SourceDefinitions;
using Espada.Infrastructure.Options;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.Text.Json;

namespace Espada.Infrastructure.Ingestion;

internal sealed class ApprovedMcpConnectorSourceClient(IOptions<ConnectorRuntimeOptions> options) : IConnectorSourceClient
{
    private readonly ConnectorRuntimeOptions _options = options.Value;

    public async Task<string> ReadAsync(ConnectorSourceDefinition definition, CancellationToken cancellationToken = default)
    {
        ApprovedConnectorOptions approved = _options.Approved.SingleOrDefault(candidate => 
                                                candidate.PluginId.Equals(definition.PluginId, StringComparison.Ordinal) && candidate.Version.Equals(definition.Version, StringComparison.Ordinal))
                                            ?? throw new IngestionException(JobFailureCategoryType.Permanent, "connector_not_approved", $"Connector '{definition.PluginId}@{definition.Version}' is not approved.");
       
        if (string.IsNullOrWhiteSpace(approved.Command))
        {
            throw new IngestionException(JobFailureCategoryType.Poison, "connector_configuration_invalid", "Approved connector command is not configured.");
        }

        StdioClientTransport transport = new(new StdioClientTransportOptions
        {
            Name = $"{definition.PluginId}@{definition.Version}",
            Command = approved.Command,
            Arguments = approved.Arguments,
            ShutdownTimeout = TimeSpan.FromSeconds(5)
        });

        await using McpClient client = await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);

        Dictionary<string, object?> arguments = definition.Arguments.ValueKind == JsonValueKind.Object
                ? definition.Arguments.Deserialize<Dictionary<string, object?>>() ?? new Dictionary<string, object?>()
                : throw new IngestionException(JobFailureCategoryType.Permanent, "connector_arguments_invalid", "Connector arguments must be a JSON object.");

        CallToolResult result = await client.CallToolAsync(definition.Resource, arguments, cancellationToken: cancellationToken);
        if (result.IsError is true)
        {
            throw new IngestionException(JobFailureCategoryType.Permanent, "connector_call_failed", "Approved connector rejected the resource request.");
        }

        string text = string.Join(Environment.NewLine, result.Content.OfType<TextContentBlock>().Select(block => block.Text));

        return string.IsNullOrWhiteSpace(text) ? throw new IngestionException( JobFailureCategoryType.Permanent, "connector_empty_result", "Approved connector returned no text content.") : text;
    }
}