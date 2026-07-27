namespace Espada.Application.Models;

public sealed record GeneratedEmbedding(
    IReadOnlyList<float> Vector,
    long InputUnits = 0);