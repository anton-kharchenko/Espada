using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ProjectTask = Espada.Domain.Aggregates.ProjectTask;

namespace Espada.Tests.Infrastructure.Database
{
    public sealed class CanonicalModelConfigurationTests
    {
        public static TheoryData<Type, string> CanonicalTables => new()
        {
            { typeof(Organization), DbTableConstants.Organizations },
            { typeof(OrganizationMembership), DbTableConstants.OrganizationMemberships },
            { typeof(Project), DbTableConstants.Projects },
            { typeof(ProjectTask), DbTableConstants.Tasks },
            { typeof(Binding), DbTableConstants.Bindings },
            { typeof(InstructionRule), DbTableConstants.InstructionRules },
            { typeof(PolicyRule), DbTableConstants.PolicyRules },
            { typeof(MemoryMetadata), DbTableConstants.MemoryMetadata }
        };

        [Theory]
        [MemberData(nameof(CanonicalTables))]
        public void Model_ShouldMapCanonicalTypes(Type entityType, string tableName)
        {
            using EspadaDbContext context = CreateContext();
            IEntityType metadata = Assert.IsAssignableFrom<IEntityType>(context.Model.FindEntityType(entityType));
            Assert.Equal(tableName, metadata.GetTableName());
            Assert.Equal(DbConstants.SchemaName, metadata.GetSchema());
        }

        [Fact]
        public void ArtifactKind_ShouldBeRequiredCanonicalStringColumn()
        {
            using EspadaDbContext context = CreateContext();
            IProperty property = Assert.IsAssignableFrom<IProperty>(context.Model.FindEntityType(typeof(Artifact))
                ?.FindProperty(nameof(Artifact.KindType)));
            Assert.False(property.IsNullable);
            Assert.Equal(DbTextColumnTypeConstants.Varchar32, property.GetColumnType());
        }

        [Fact]
        public void BindingWorkspaceSelector_ShouldBeRequired()
        {
            using EspadaDbContext context = CreateContext();
            IEntityType metadata = Assert.IsAssignableFrom<IEntityType>(context.Model.FindEntityType(typeof(Binding)));
            IForeignKey foreignKey = Assert.Single(metadata.GetForeignKeys(),
                candidate => candidate.PrincipalEntityType.ClrType == typeof(Workspace));
            Assert.True(foreignKey.IsRequired);
            Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
        }

        [Fact]
        public void WorkspaceOrganizationOwnership_ShouldBeOptionalAndRestricted()
        {
            using EspadaDbContext context = CreateContext();
            IEntityType metadata = Assert.IsAssignableFrom<IEntityType>(
                context.Model.FindEntityType(typeof(Workspace)));
            IProperty organizationId = Assert.IsAssignableFrom<IProperty>(
                metadata.FindProperty(nameof(Workspace.OrganizationId)));
            IForeignKey foreignKey = Assert.Single(
                metadata.GetForeignKeys(),
                candidate => candidate.PrincipalEntityType.ClrType
                             == typeof(Organization));

            Assert.True(organizationId.IsNullable);
            Assert.False(foreignKey.IsRequired);
            Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
            Assert.Contains(
                metadata.GetIndexes(),
                index => index.GetDatabaseName()
                         == "IX_Workspaces_OrganizationId");
        }

        [Fact]
        public void Rules_ShouldUseRevisionAndStableKeyAsPrimaryKey()
        {
            using EspadaDbContext context = CreateContext();
            string[] instructionKey = context.Model.FindEntityType(typeof(InstructionRule))!.FindPrimaryKey()!
                .Properties.Select(property => property.Name).ToArray();
            string[] policyKey = context.Model.FindEntityType(typeof(PolicyRule))!.FindPrimaryKey()!.Properties
                .Select(property => property.Name).ToArray();
            Assert.Equal([nameof(InstructionRule.ArtifactRevisionId), nameof(InstructionRule.RuleKey)], instructionKey);
            Assert.Equal([nameof(PolicyRule.ArtifactRevisionId), nameof(PolicyRule.RuleKey)], policyKey);
        }

        [Fact]
        public void ArtifactRevision_ShouldReferenceArtifactWithinSameWorkspace()
        {
            using EspadaDbContext context = CreateContext();
            IEntityType metadata =
                Assert.IsAssignableFrom<IEntityType>(context.Model.FindEntityType(typeof(ArtifactRevision)));
            IForeignKey foreignKey = Assert.Single(metadata.GetForeignKeys(),
                candidate => candidate.PrincipalEntityType.ClrType == typeof(Artifact));

            Assert.Equal([nameof(ArtifactRevision.ArtifactId), nameof(ArtifactRevision.WorkspaceId)],
                foreignKey.Properties.Select(property => property.Name));
            Assert.Equal([nameof(Artifact.Id), nameof(Artifact.WorkspaceId)],
                foreignKey.PrincipalKey.Properties.Select(property => property.Name));
        }

        [Fact]
        public void Binding_ShouldUseWorkspaceScopedRevisionProjectAndTaskForeignKeys()
        {
            using EspadaDbContext context = CreateContext();
            IEntityType metadata = Assert.IsAssignableFrom<IEntityType>(context.Model.FindEntityType(typeof(Binding)));

            AssertForeignKey(metadata, typeof(ArtifactRevision), nameof(Binding.ArtifactRevisionId),
                nameof(Binding.WorkspaceId));
            AssertForeignKey(metadata, typeof(Project), nameof(Binding.ProjectId), nameof(Binding.WorkspaceId));
            AssertForeignKey(metadata, typeof(ProjectTask), nameof(Binding.TaskId), nameof(Binding.ProjectId),
                nameof(Binding.WorkspaceId));
        }

        [Fact]
        public void RulesAndMemory_ShouldUseKindAndIdentityScopedRevisionForeignKeys()
        {
            using EspadaDbContext context = CreateContext();

            AssertForeignKey(
                Assert.IsAssignableFrom<IEntityType>(context.Model.FindEntityType(typeof(InstructionRule))),
                typeof(ArtifactRevision), nameof(InstructionRule.ArtifactRevisionId), nameof(InstructionRule.KindType));
            AssertForeignKey(Assert.IsAssignableFrom<IEntityType>(context.Model.FindEntityType(typeof(PolicyRule))),
                typeof(ArtifactRevision), nameof(PolicyRule.ArtifactRevisionId), nameof(PolicyRule.KindType));
            AssertForeignKey(Assert.IsAssignableFrom<IEntityType>(context.Model.FindEntityType(typeof(MemoryMetadata))),
                typeof(ArtifactRevision), nameof(MemoryMetadata.ArtifactRevisionId), nameof(MemoryMetadata.ArtifactId),
                nameof(MemoryMetadata.KindType));
        }

        private static void AssertForeignKey(IEntityType dependent, Type principalType, params string[] properties)
        {
            IForeignKey foreignKey = Assert.Single(dependent.GetForeignKeys(),
                candidate => candidate.PrincipalEntityType.ClrType == principalType &&
                             candidate.Properties.Select(property => property.Name).SequenceEqual(properties));
            Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
        }

        private static EspadaDbContext CreateContext()
        {
            return new EspadaDbContext(
                PostgreSqlDbContextOptions.Create<EspadaDbContext>(ModelTestDatabase.ConnectionString));
        }
    }
}