namespace Espada.Infrastructure.Options;

public sealed class ApprovedConnectorOptions
{
    public string PluginId { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string Command { get; set; } = string.Empty;

    public List<string> Arguments { get; set; } = [];
}