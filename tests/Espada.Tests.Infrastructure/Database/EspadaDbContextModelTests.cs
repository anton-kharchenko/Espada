using Espada.Db.Constants;
using Espada.Db.Database;
using Espada.Db.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.SeedWork;
using Espada.Infrastructure.Database;
using Espada.Tests.Infrastructure.TestData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

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

            Assert.Equal(DbTableConstants.Workspaces, context.Model.FindEntityType(typeof(Workspace))?.GetTableName());
            Assert.Equal(DbTableConstants.Sources, context.Model.FindEntityType(typeof(Source))?.GetTableName());
            Assert.Equal(DbTableConstants.ImportJobs, context.Model.FindEntityType(typeof(ImportJob))?.GetTableName());
            Assert.Equal(DbTableConstants.Artifacts, context.Model.FindEntityType(typeof(Artifact))?.GetTableName());
            Assert.Equal(DbTableConstants.ArtifactRevisions,
                context.Model.FindEntityType(typeof(ArtifactRevision))?.GetTableName());
            Assert.Equal(DbTableConstants.ChunkBatches,
                context.Model.FindEntityType(typeof(ChunkBatch))?.GetTableName());
            Assert.Equal(DbTableConstants.Chunks, context.Model.FindEntityType(typeof(Chunk))?.GetTableName());
            Assert.Equal(DbTableConstants.ChunkEmbeddings,
                context.Model.FindEntityType(typeof(ChunkEmbedding))?.GetTableName());
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
            Assert.Equal(DbIndexConstants.ArtifactRevisionArtifactNumber, index.GetDatabaseName());
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
            Assert.Equal(DbIndexConstants.SourceWorkspaceLocator, index.GetDatabaseName());
        }

        [Theory]
        [MemberData(nameof(RequiredForeignKeyTestData.Relationships), MemberType = typeof(RequiredForeignKeyTestData))]
        public void Model_ShouldConfigureRequiredForeignKey(Type dependentType, string foreignKeyProperty,
            Type principalType)
        {
            using EspadaDbContext context = CreateContext();

            IEntityType entityType = Assert.IsAssignableFrom<IEntityType>(context.Model.FindEntityType(dependentType));

            IForeignKey? foreignKey = entityType.GetForeignKeys().SingleOrDefault(candidate =>
                candidate.PrincipalEntityType.ClrType == principalType &&
                candidate.Properties.Count == 1 &&
                candidate.Properties[0].Name == foreignKeyProperty);

            Assert.NotNull(foreignKey);
            Assert.True(foreignKey.IsRequired);
            Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
        }

        [MemberData(nameof(MutableAggregateTestData.Types), MemberType = typeof(MutableAggregateTestData))]
        [Theory]
        public void MutableAggregate_ShouldUsePostgreSqlRowVersion(Type aggregateType)
        {
            using EspadaDbContext context = CreateContext();

            IEntityType entityType = Assert.IsAssignableFrom<IEntityType>(
                context.Model.FindEntityType(aggregateType));
            IProperty property = Assert.IsAssignableFrom<IProperty>(
                entityType.FindProperty(nameof(IHasConcurrencyVersion.Version)));

            Assert.Equal(typeof(uint), property.ClrType);
            Assert.Equal("xmin", property.GetColumnName());
            Assert.Equal("xid", property.GetColumnType());
            Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
            Assert.True(property.IsConcurrencyToken);
        }

        [Fact]
        public void MigrationModels_ShouldDeclareStoreTypeForEveryPhysicalProperty()
        {
            string migrationModelNamespace = typeof(Workspaces).Namespace!;
            string[] missingMappings = typeof(SetupDbContext).Assembly
                .GetTypes()
                .Where(type => type.Namespace == migrationModelNamespace)
                .SelectMany(type => type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                .Where(property => IsPhysicalColumn(property, migrationModelNamespace))
                .Where(property => string.IsNullOrWhiteSpace(
                    property.GetCustomAttribute<ColumnAttribute>()?.TypeName))
                .Select(property => $"{property.DeclaringType!.Name}.{property.Name}")
                .Order(StringComparer.Ordinal)
                .ToArray();

            Assert.Empty(missingMappings);
        }

        [Fact]
        public void SetupAndRuntimeModels_ShouldUseSameColumnTypesForSharedTables()
        {
            using SetupDbContext setupContext = CreateSetupContext();
            using EspadaDbContext runtimeContext = CreateContext();

            IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> setupTables =
                GetTableColumnTypes(setupContext.Model);
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> runtimeTables =
                GetTableColumnTypes(runtimeContext.Model);
            string[] sharedTables = setupTables.Keys
                .Intersect(runtimeTables.Keys, StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

            Assert.NotEmpty(sharedTables);
            foreach (string table in sharedTables)
            {
                KeyValuePair<string, string>[] setupColumns = setupTables[table]
                    .OrderBy(column => column.Key, StringComparer.Ordinal)
                    .ToArray();
                KeyValuePair<string, string>[] runtimeColumns = runtimeTables[table]
                    .OrderBy(column => column.Key, StringComparer.Ordinal)
                    .ToArray();

                Assert.Equal(setupColumns, runtimeColumns);
            }
        }

        private static bool IsPhysicalColumn(
            PropertyInfo property,
            string migrationModelNamespace)
        {
            Type propertyType = Nullable.GetUnderlyingType(property.PropertyType)
                                ?? property.PropertyType;

            return propertyType.Namespace != migrationModelNamespace;
        }

        private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>
            GetTableColumnTypes(IModel model)
        {
            Dictionary<string, Dictionary<string, string>> tables =
                new(StringComparer.Ordinal);

            foreach (IEntityType entityType in model.GetEntityTypes())
            {
                string? tableName = entityType.GetTableName();
                if (tableName is null)
                {
                    continue;
                }

                string? schema = entityType.GetSchema() ?? model.GetDefaultSchema();
                string tableKey = $"{schema}.{tableName}";
                StoreObjectIdentifier table = StoreObjectIdentifier.Table(tableName, schema);
                if (!tables.TryGetValue(tableKey, out Dictionary<string, string>? columns))
                {
                    columns = new Dictionary<string, string>(StringComparer.Ordinal);
                    tables.Add(tableKey, columns);
                }

                foreach (IProperty property in entityType.GetProperties())
                {
                    string? columnName = property.GetColumnName(table);
                    string? columnType = property.GetColumnType();
                    if (columnName is not null && columnType is not null)
                    {
                        columns[columnName] = columnType;
                    }
                }
            }

            return tables.ToDictionary(
                table => table.Key,
                table => (IReadOnlyDictionary<string, string>)table.Value,
                StringComparer.Ordinal);
        }

        private static SetupDbContext CreateSetupContext()
        {
            return new SetupDbContext(
                PostgreSqlDbContextOptions.Create<SetupDbContext>(
                    ModelTestDatabase.ConnectionString));
        }

        private static EspadaDbContext CreateContext()
        {
            return new EspadaDbContext(
                PostgreSqlDbContextOptions.Create<EspadaDbContext>(ModelTestDatabase.ConnectionString));
        }
    }
}