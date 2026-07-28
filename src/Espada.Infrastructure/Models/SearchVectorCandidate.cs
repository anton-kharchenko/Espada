namespace Espada.Infrastructure.Models
{
    internal sealed record SearchVectorCandidate(
        Guid ChunkId,
        double Similarity);
}