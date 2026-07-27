namespace Espada.Protocol.Mcp.Contracts.Responses;

public sealed record ContextSearchResponse(IReadOnlyList<ContextSearchItem> Items);