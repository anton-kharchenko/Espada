namespace Espada.AgentAdapters.Context
{
    public sealed record AgentContextProjection(
        string Agent,
        string Format,
        string MediaType,
        string Content,
        int SizeInBytes);
}
