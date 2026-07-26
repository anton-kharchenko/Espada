using Espada.Domain.Aggregates;
using Espada.Domain.SeedWork;
using Espada.Infrastructure.Database;
using Espada.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Espada.Tests.Infrastructure.Database
{
    public sealed class EspadaDbContextModelTests
    {
        [Fact]
        public void Model_ShouldContainConfiguredAggregateTypes()
        {
            using EspadaDbContext context = CreateContext();

            IModel model = context.Model;

            Assert.NotNull(model.FindEntityType(typeof(Workspace)));
            Assert.NotNull(model.FindEntityType(typeof(Source)));
            Assert.NotNull(model.FindEntityType(typeof(ImportJob)));
            Assert.NotNull(model.FindEntityType(typeof(Artifact)));
            Assert.NotNull(model.FindEntityType(typeof(ArtifactRevision)));
            Assert.NotNull(model.FindEntityType(typeof(ChunkBatch)));
            Assert.NotNull(model.FindEntityType(typeof(Chunk)));
            Assert.NotNull(model.FindEntityType(typeof(ChunkEmbedding)));
        }

        [Fact]
        public void Model_ShouldUseExpectedTableNames()
        {
            using EspadaDbContext context = CreateContext();

            Assert.Equal(DbConstants.Tables.Workspaces, context.Model.FindEntityType(typeof(Workspace))?.GetTableName());
            Assert.Equal(DbConstants.Tables.Sources, context.Model.FindEntityType(typeof(Source))?.GetTableName());
            Assert.Equal(DbConstants.Tables.ImportJobs, context.Model.FindEntityType(typeof(ImportJob))?.GetTableName());
            Assert.Equal(DbConstants.Tables.Artifacts, context.Model.FindEntityType(typeof(Artifact))?.GetTableName());
            Assert.Equal(DbConstants.Tables.ArtifactRevisions, context.Model.FindEntityType(typeof(ArtifactRevision))?.GetTableName());
            Assert.Equal(DbConstants.Tables.ChunkBatches, context.Model.FindEntityType(typeof(ChunkBatch))?.GetTableName());
            Assert.Equal(DbConstants.Tables.Chunks, context.Model.FindEntityType(typeof(Chunk))?.GetTableName());
            Assert.Equal(DbConstants.Tables.ChunkEmbeddings, context.Model.FindEntityType(typeof(ChunkEmbedding))?.GetTableName());
        }

        [Fact]
        public void ArtifactRevision_ShouldHaveUniqueArtifactNumberIndex()
        {
            using EspadaDbContext context = CreateContext();

            IEntityType entityType = Assert.IsAssignableFrom<IEntityType>(
                context.Model.FindEntityType(typeof(ArtifactRevision)));

            IIndex? index = entityType.GetIndexes().SingleOrDefault(candidate =>
                candidate.Properties.Select(property => property.Name)
                    .SequenceEqual([
                        nameof(ArtifactRevision.ArtifactId),
                        nameof(ArtifactRevision.Number)
                    ]));

            Assert.NotNull(index);
            Assert.True(index.IsUnique);
            Assert.Equal(DbConstants.Indexes.ArtifactRevisionArtifactNumber, index.GetDatabaseName());
        }

        [Fact]
        public void Source_ShouldHaveUniqueWorkspaceLocatorIndex()
        {
            using EspadaDbContext context = CreateContext();

            IEntityType entityType = Assert.IsAssignableFrom<IEntityType>(
                context.Model.FindEntityType(typeof(Source)));

            IIndex? index = entityType.GetIndexes().SingleOrDefault(candidate =>
                candidate.Properties.Select(property => property.Name)
                    .SequenceEqual([
                        nameof(Source.WorkspaceId),
                        nameof(Source.Locator)
                    ]));

            Assert.NotNull(index);
            Assert.True(index.IsUnique);
            Assert.Equal(DbConstants.Indexes.SourceWorkspaceLocator, index.GetDatabaseName());
        }

        [Theory]
        [InlineData(typeof(Source), nameof(Source.WorkspaceId), typeof(Workspace))]
        [InlineData(typeof(ImportJob), nameof(ImportJob.SourceId), typeof(Source))]
        [InlineData(typeof(ImportJob), nameof(ImportJob.WorkspaceId), typeof(Workspace))]
        [InlineData(typeof(Artifact), nameof(Artifact.WorkspaceId), typeof(Workspace))]
        [InlineData(typeof(ArtifactRevision), nameof(ArtifactRevision.ArtifactId), typeof(Artifact))]
        [InlineData(typeof(ChunkBatch), nameof(ChunkBatch.ArtifactRevisionId), typeof(ArtifactRevision))]
        [InlineData(typeof(Chunk), nameof(Chunk.BatchId), typeof(ChunkBatch))]
        [InlineData(typeof(Chunk), nameof(Chunk.ArtifactId), typeof(Artifact))]
        [InlineData(typeof(Chunk), nameof(Chunk.ArtifactRevisionId), typeof(ArtifactRevision))]
        [InlineData(typeof(ChunkEmbedding), nameof(ChunkEmbedding.ChunkId), typeof(Chunk))]
        public void Model_ShouldConfigureRequiredForeignKey(
            Type dependentType,
            string foreignKeyProperty,
            Type principalType)
        {
            using EspadaDbContext context = CreateContext();

            IEntityType entityType = Assert.IsAssignableFrom<IEntityType>(
                context.Model.FindEntityType(dependentType));

            IForeignKey? foreignKey = entityType.GetForeignKeys().SingleOrDefault(candidate =>
                candidate.PrincipalEntityType.ClrType == principalType &&
                candidate.Properties.Count == 1 &&
                candidate.Properties[0].Name == foreignKeyProperty);

            Assert.NotNull(foreignKey);
            Assert.True(foreignKey.IsRequired);
            Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
        }

        [Theory]
        [InlineData(typeof(Workspace))]
        [InlineData(typeof(Source))]
        [InlineData(typeof(ImportJob))]
        [InlineData(typeof(Artifact))]
        [InlineData(typeof(ChunkBatch))]
        public void MutableAggregate_ShouldUseVersionConcurrencyToken(Type aggregateType)
        {
            using EspadaDbContext context = CreateContext();

            IEntityType entityType = Assert.IsAssignableFrom<IEntityType>(
                context.Model.FindEntityType(aggregateType));
            IProperty property = Assert.IsAssignableFrom<IProperty>(
                entityType.FindProperty(nameof(IHasConcurrencyVersion.Version)));

            Assert.Equal(typeof(long), property.ClrType);
            Assert.True(property.IsConcurrencyToken);
        }

        private static EspadaDbContext CreateContext()
        {
            DbContextOptions<EspadaDbContext> options =
                new DbContextOptionsBuilder<EspadaDbContext>()
                    .UseNpgsql(
                        "Host=localhost;Port=5432;Database=espada_model_tests;Username=postgres;Password=postgres")
                    .Options;

            return new EspadaDbContext(options);
        }
    }
}