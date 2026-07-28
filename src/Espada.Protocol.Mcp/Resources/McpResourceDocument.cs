namespace Espada.Protocol.Mcp.Resources
{
    internal sealed record McpResourceDocument<TData>(
        string MediaType,
        McpResourceProvenance Provenance,
        TData Data);
}