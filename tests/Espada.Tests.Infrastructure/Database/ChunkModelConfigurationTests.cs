using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Infrastructure.Database;
using Espada.Tests.Common.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Espada.Tests.Infrastructure.Database;

public sealed class ChunkModelConfigurationTests
{
    [Fact]
    public void Model_ShouldContainChunkAggregateTypes()
    {
        using EspadaDbContext context = CreateContext();

        Assert.NotNull(context.Model.FindEntityType(typeof(ChunkBatch)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Chunk)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ChunkEmbedding)));
    }

    [Theory]
    [InlineData(typeof(ChunkBatch), DbTableConstants.ChunkBatches)]
    [InlineData(typeof(Chunk), DbTableConstants.Chunks)]
    [InlineData(typeof(ChunkEmbedding), DbTableConstants.ChunkEmbeddings)]
    public void Model_ShouldUseExpectedTableAndSchema(Type entityType, string tableName)
    {
        using EspadaDbContext context = CreateContext();

        IEntityType metadata = Assert.IsAssignableFrom<IEntityType>(
            context.Model.FindEntityType(entityType));

        Assert.Equal(tableName, metadata.GetTableName());
        Assert.Equal(DbConstants.SchemaName, metadata.GetSchema());
    }

    [Fact]
    public void Chunk_ShouldHaveUniqueBatchNumberIndex()
    {
        using EspadaDbContext context = CreateContext();

        IEntityType metadata = Assert.IsAssignableFrom<IEntityType>(
            context.Model.FindEntityType(typeof(Chunk)));

        IIndex? index = metadata.GetIndexes().SingleOrDefault(candidate =>
            candidate.Properties.Select(property => property.Name)
                .SequenceEqual(new[]
                {
                    nameof(Chunk.BatchId),
                    nameof(Chunk.Number)
                }));

        Assert.NotNull(index);
        Assert.True(index.IsUnique);
        Assert.Equal(DbIndexConstants.ChunkBatchNumber, index.GetDatabaseName());
    }

    [Fact]
    public void ChunkEmbedding_ShouldHaveUniqueChunkModelIndex()
    {
        using EspadaDbContext context = CreateContext();

        IEntityType metadata = Assert.IsAssignableFrom<IEntityType>(
            context.Model.FindEntityType(typeof(ChunkEmbedding)));

        IIndex? index = metadata.GetIndexes().SingleOrDefault(candidate =>
            candidate.Properties.Select(property => property.Name)
                .SequenceEqual(new[]
                {
                    nameof(ChunkEmbedding.ChunkId),
                    DbPropertyConstants.ChunkEmbeddingModelIdentifier,
                    DbPropertyConstants.ChunkEmbeddingModelVersion
                }));

        Assert.NotNull(index);
        Assert.True(index.IsUnique);
        Assert.Equal(DbIndexConstants.ChunkEmbeddingChunkModel, index.GetDatabaseName());
    }

    private static EspadaDbContext CreateContext()
    {
        return new EspadaDbContext(
            PostgreSqlDbContextOptions.Create<EspadaDbContext>(
                ModelTestDatabase.ConnectionString));
    }
}