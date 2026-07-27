using Espada.Domain.Aggregates;

namespace Espada.Tests.Infrastructure.TestData;

internal static class MutableAggregateTestData
{
    public static TheoryData<Type> Types =>
    [
        typeof(Workspace),
        typeof(Source),
        typeof(ImportJob),
        typeof(Artifact),
        typeof(ChunkBatch)
    ];
}