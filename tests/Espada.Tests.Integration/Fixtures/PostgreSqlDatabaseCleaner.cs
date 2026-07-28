using Espada.Db.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Espada.Tests.Integration.Fixtures
{
    internal static class PostgreSqlDatabaseCleaner
    {
        public static async Task ResetAsync(SetupDbContext dbContext, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dbContext);

            await using IDbContextTransaction transaction =
                await dbContext.Database.BeginTransactionAsync(cancellationToken);

            await dbContext.OneTimeBootstrapCodes.ExecuteDeleteAsync(cancellationToken);
            await dbContext.UsageReconciliationOutbox.ExecuteDeleteAsync(cancellationToken);
            await dbContext.UsageLedgerEntries.ExecuteDeleteAsync(cancellationToken);
            await dbContext.PaymentEvents.ExecuteDeleteAsync(cancellationToken);
            await dbContext.IngestionJobs.ExecuteDeleteAsync(cancellationToken);
            await dbContext.OutboxMessages.ExecuteDeleteAsync(cancellationToken);
            await dbContext.EmbeddingVectors.ExecuteDeleteAsync(cancellationToken);
            await dbContext.ChunkEmbeddings.ExecuteDeleteAsync(cancellationToken);
            await dbContext.Chunks.ExecuteDeleteAsync(cancellationToken);
            await dbContext.ChunkBatches.ExecuteDeleteAsync(cancellationToken);
            await dbContext.ImportJobs.ExecuteDeleteAsync(cancellationToken);
            await dbContext.Bindings.ExecuteDeleteAsync(cancellationToken);
            await dbContext.InstructionRules.ExecuteDeleteAsync(cancellationToken);
            await dbContext.PolicyRules.ExecuteDeleteAsync(cancellationToken);
            await dbContext.MemoryMetadata.ExecuteDeleteAsync(cancellationToken);
            await dbContext.ArtifactRevisions.ExecuteDeleteAsync(cancellationToken);
            await dbContext.Artifacts.ExecuteDeleteAsync(cancellationToken);
            await dbContext.Tasks.ExecuteDeleteAsync(cancellationToken);
            await dbContext.Projects.ExecuteDeleteAsync(cancellationToken);
            await dbContext.Sources.ExecuteDeleteAsync(cancellationToken);
            await dbContext.BillingCustomers.ExecuteDeleteAsync(cancellationToken);
            await dbContext.WorkspaceMemberships.ExecuteDeleteAsync(cancellationToken);
            await dbContext.Workspaces.ExecuteDeleteAsync(cancellationToken);
            await dbContext.OrganizationMemberships.ExecuteDeleteAsync(cancellationToken);
            await dbContext.Organizations.ExecuteDeleteAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
    }
}
