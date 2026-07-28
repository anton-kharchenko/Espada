using Espada.Domain.Enums;
using System.Text.Json;

namespace Espada.Domain.ValueObjects.SourceDefinitions
{
    public sealed record ConnectorSourceDefinition : SourceDefinition
    {
        public ConnectorSourceDefinition(string pluginId, string version, string resource, JsonElement arguments)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);
            ArgumentException.ThrowIfNullOrWhiteSpace(version);
            ArgumentException.ThrowIfNullOrWhiteSpace(resource);

            PluginId = pluginId;
            Version = version;
            Resource = resource;
            Arguments = arguments.Clone();
        }

        public string PluginId { get; init; }

        public string Version { get; init; }

        public string Resource { get; init; }

        public JsonElement Arguments { get; init; }

        public override SourceType SourceType => SourceType.Connector;

        public override string CanonicalLocator => $"connector:{PluginId}:{Version}:{Resource}";
    }
}