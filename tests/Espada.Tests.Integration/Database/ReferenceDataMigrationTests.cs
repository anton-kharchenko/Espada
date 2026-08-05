using Espada.Db.Database;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Espada.Tests.Integration.Database
{
    [Collection(PostgreSqlIntegrationCollection.Name)]
    public sealed class ReferenceDataMigrationTests(PostgreSqlDatabaseFixture fixture)
        : PostgreSqlIntegrationTest(fixture)
    {
        [Fact]
        public async Task Migrations_ShouldSeedReferenceTablesFromDomainEnumerations()
        {
            await using SetupDbContext dbContext = Fixture.CreateSetupDbContext();
            CancellationToken cancellationToken = TestContext.Current.CancellationToken;

            Assert.Equal(Expected<WorkspaceType>(),
                (await dbContext.WorkspaceTypes.OrderBy(model => model.WorkspaceTypeId).ToListAsync(cancellationToken))
                .Select(model => (model.WorkspaceTypeId, model.Name)));
            Assert.Equal(Expected<WorkspaceStatusType>(),
                (await dbContext.WorkspaceStatusTypes.OrderBy(model => model.WorkspaceStatusTypeId)
                    .ToListAsync(cancellationToken)).Select(model => (model.WorkspaceStatusTypeId, model.Name)));
            Assert.Equal(Expected<SourceType>(),
                (await dbContext.SourceTypes.OrderBy(model => model.SourceTypeId).ToListAsync(cancellationToken))
                .Select(model => (model.SourceTypeId, model.Name)));
            Assert.Equal(Expected<SourceStatusType>(),
                (await dbContext.SourceStatusTypes.OrderBy(model => model.SourceStatusTypeId)
                    .ToListAsync(cancellationToken)).Select(model => (model.SourceStatusTypeId, model.Name)));
            Assert.Equal(Expected<ImportStatusType>(),
                (await dbContext.ImportStatusTypes.OrderBy(model => model.ImportStatusTypeId)
                    .ToListAsync(cancellationToken)).Select(model => (model.ImportStatusTypeId, model.Name)));
            Assert.Equal(Expected<ArtifactType>(),
                (await dbContext.ArtifactTypes.OrderBy(model => model.ArtifactTypeId).ToListAsync(cancellationToken))
                .Select(model => (model.ArtifactTypeId, model.Name)));
            Assert.Equal(Expected<ArtifactStatusType>(),
                (await dbContext.ArtifactStatusTypes.OrderBy(model => model.ArtifactStatusTypeId)
                    .ToListAsync(cancellationToken)).Select(model => (model.ArtifactStatusTypeId, model.Name)));
            Assert.Equal(Expected<ChunkingStrategyType>(),
                (await dbContext.ChunkingStrategyTypes.OrderBy(model => model.ChunkingStrategyTypeId)
                    .ToListAsync(cancellationToken)).Select(model => (model.ChunkingStrategyTypeId, model.Name)));
            Assert.Equal(Expected<ChunkBatchStatusType>(),
                (await dbContext.ChunkBatchStatusTypes.OrderBy(model => model.ChunkBatchStatusTypeId)
                    .ToListAsync(cancellationToken)).Select(model => (model.ChunkBatchStatusTypeId, model.Name)));
        }

        private static IEnumerable<(int Id, string Name)> Expected<TEnumeration>() where TEnumeration : Enumeration
        {
            return Enumeration.GetAll<TEnumeration>().OrderBy(value => value.Id)
                .Select(value => (value.Id, value.Name));
        }
    }
}