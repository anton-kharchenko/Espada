namespace Espada.Application.Models
{
    public sealed record ChunkSegment(int Number, string Content, int Start, int Length);
}