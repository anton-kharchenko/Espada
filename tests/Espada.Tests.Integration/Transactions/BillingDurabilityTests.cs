using Espada.Application.Contracts.Billing;
using Espada.Application.Contracts.Billing.Constants;
using Espada.Application.Contracts.Persistence;
using Espada.Billing;
using Espada.Billing.Constants;
using Espada.Billing.Contracts;
using Espada.Billing.Extensions;
using Espada.Infrastructure.Database;
using Espada.Tests.Integration.Database;
using Espada.Tests.Integration.Fixtures;
using Espada.Tests.Integration.TestData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stripe;
using System.Text.Json;

namespace Espada.Tests.Integration.Transactions;

[Collection(PostgreSqlIntegrationCollection.Name)]
public sealed class BillingDurabilityTests(PostgreSqlDatabaseFixture fixture) : PostgreSqlIntegrationTest(fixture)
{
    [Fact]
    public async Task SignedWebhook_ShouldBeDurableAndDeduplicated()
    {
        PersistenceGraph graph = PersistenceGraphFactory.Create();
        await using (EspadaDbContext dbContext = Fixture.CreateDbContext())
        {
            dbContext.Workspaces.Add(graph.Workspace);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        IConfiguration configuration = BillingTestConfigurationFactory.Create();
        await using ServiceProvider serviceProvider =
            Fixture.CreateServiceProvider(
                configuration,
                services => services.AddEspadaBilling(configuration));
        using IServiceScope scope = serviceProvider.CreateScope();
        IStripeWebhookIngestor ingestor = scope.ServiceProvider.GetRequiredService<IStripeWebhookIngestor>();

        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string eventId = $"evt_{Guid.NewGuid():N}";
        string payload = JsonSerializer.Serialize(new
        {
            id = eventId,
            @object = "event",
            api_version = BillingConstants.RequiredStripeApiVersion,
            created = timestamp,
            data = new { @object = new { id = "cus_test", @object = "customer" } },
            livemode = false,
            pending_webhooks = 1,
            type = "customer.created"
        });
        string secret = "whsec_integration";
        string signature = EventUtility.ComputeSignature(
            secret,
            timestamp.ToString(
                System.Globalization.CultureInfo.InvariantCulture),
            payload);
        string header = $"t={timestamp},v1={signature}";

        Assert.True(
            await ingestor.AcceptAsync(
                payload,
                header,
                TestContext.Current.CancellationToken));
        Assert.False(
            await ingestor.AcceptAsync(
                payload,
                header,
                TestContext.Current.CancellationToken));

        IStripeWebhookProcessor processor =
            scope.ServiceProvider
                .GetRequiredService<IStripeWebhookProcessor>();
        Assert.True(
            await processor.ProcessNextAsync(
                "billing-worker",
                TestContext.Current.CancellationToken));
        Assert.False(
            await processor.ProcessNextAsync(
                "billing-worker",
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task UsageMeter_ShouldCommitLedgerAndReconciliationTogether()
    {
        PersistenceGraph graph = PersistenceGraphFactory.Create();
        IConfiguration configuration = BillingTestConfigurationFactory.Create();
        await using ServiceProvider serviceProvider =
            Fixture.CreateServiceProvider(
                configuration,
                services => services.AddEspadaBilling(configuration));
        using IServiceScope scope = serviceProvider.CreateScope();
        EspadaDbContext dbContext = scope.ServiceProvider.GetRequiredService<EspadaDbContext>();
        dbContext.Workspaces.Add(graph.Workspace);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        IUsageMeterService meterService =
            scope.ServiceProvider.GetRequiredService<IUsageMeterService>();
        await meterService.RecordAsync(
            graph.Workspace.Id.Value,
            UsageMetricConstants.RawBytes,
            42,
            "usage-test",
            TestContext.Current.CancellationToken);
        await scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>()
            .SaveChangesAsync(TestContext.Current.CancellationToken);

        await using Espada.Db.Database.SetupDbContext readContext =
            Fixture.CreateSetupDbContext();
        long reconciliationCount = await readContext.UsageLedgerEntries
            .Where(entry => entry.IdempotencyKey == "usage-test")
            .Join(
                readContext.UsageReconciliationOutbox,
                entry => entry.EntryId,
                message => message.LedgerEntryId,
                (_, _) => 1)
            .LongCountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, reconciliationCount);
    }
}