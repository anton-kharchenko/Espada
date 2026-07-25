using Espada.Domain.Aggregates;
using Espada.Infrastructure.Database;
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
        }

        [Fact]
        public void Model_ShouldUseExpectedTableNames()
        {
            using EspadaDbContext context = CreateContext();

            Assert.Equal("Workspaces", context.Model.FindEntityType(typeof(Workspace))?.GetTableName());
            Assert.Equal("Sources", context.Model.FindEntityType(typeof(Source))?.GetTableName());
            Assert.Equal("ImportJobs", context.Model.FindEntityType(typeof(ImportJob))?.GetTableName());
            Assert.Equal("Artifacts", context.Model.FindEntityType(typeof(Artifact))?.GetTableName());
            Assert.Equal("ArtifactRevisions", context.Model.FindEntityType(typeof(ArtifactRevision))?.GetTableName());
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