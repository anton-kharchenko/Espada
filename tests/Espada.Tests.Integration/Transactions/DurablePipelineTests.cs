using Espada.Application.Contracts.Jobs;
using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.Constants;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;
using Espada.Domain.ValueObjects.SourceDefinitions;
using Espada.Infrastructure.Database;
using Espada.Tests.Integration.Database;
using Espada.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Tests.Integration.Transactions;

[Collection(PostgreSqlIntegrationCollection.Name)]
public sealed class DurablePipelineTests(PostgreSqlDatabaseFixture fixture) : PostgreSqlIntegrationTest(fixture)
{
    [Fact]
    public async Task SaveChanges_ShouldPersistAggregateAndDomainEventsAtomically()
    {
        PersistenceGraph graph = PersistenceGraphFactory.Create();
        await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();
        using IServiceScope scope = serviceProvider.CreateScope();
        EspadaDbContext dbContext = scope.ServiceProvider.GetRequiredService<EspadaDbContext>();
        dbContext.AddRange(graph.Workspace, graph.Source);
        ImportJob requested = ImportJob.Request(
            ImportJobId.New(),
            graph.Source.Id,
            graph.Workspace.Id,
            graph.ImportJob.RequestedAtUtc,
            "atomic-request",
            "sha256:atomic-request",
            "{}").ShouldSucceed();
        dbContext.ImportJobs.Add(requested);

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await using Espada.Db.Database.SetupDbContext readContext =
            Fixture.CreateSetupDbContext();
        long count = await readContext.OutboxMessages.LongCountAsync(
            message => message.EventName == DomainEventContractConstants.ImportRequested,
            TestContext.Current.CancellationToken);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Queue_ShouldDeduplicateStageAndAllowOnlyOneWorkerToClaimIt()
    {
        PersistenceGraph graph = PersistenceGraphFactory.Create();
        await using (EspadaDbContext setupContext = Fixture.CreateDbContext())
        {
            setupContext.AddRange(graph.Workspace, graph.Source, graph.ImportJob);
            await setupContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();
        using (IServiceScope enqueueScope = serviceProvider.CreateScope())
        {
            IJobQueue queue = enqueueScope.ServiceProvider.GetRequiredService<IJobQueue>();
            await queue.EnqueueAsync(
                graph.ImportJob.Id,
                ImportPipelineStageType.Read,
                "import:read",
                TestContext.Current.CancellationToken);
            await queue.EnqueueAsync(
                graph.ImportJob.Id,
                ImportPipelineStageType.Read,
                "import:read",
                TestContext.Current.CancellationToken);
        }

        using IServiceScope firstScope = serviceProvider.CreateScope();
        using IServiceScope secondScope = serviceProvider.CreateScope();
        Task<IngestionJob?> firstClaim = firstScope.ServiceProvider
            .GetRequiredService<IJobQueue>()
            .ClaimAsync(
                "worker-1",
                TimeSpan.FromMinutes(1),
                TestContext.Current.CancellationToken);
        Task<IngestionJob?> secondClaim = secondScope.ServiceProvider
            .GetRequiredService<IJobQueue>()
            .ClaimAsync(
                "worker-2",
                TimeSpan.FromMinutes(1),
                TestContext.Current.CancellationToken);

        IngestionJob?[] claims = await Task.WhenAll(firstClaim, secondClaim);
        IngestionJob claimed = Assert.Single(claims.OfType<IngestionJob>());
        Assert.Equal(ImportPipelineStageType.Read, claimed.Stage);
        Assert.Equal(graph.ImportJob.Id, claimed.ImportJobId);
    }

    [Fact]
    public async Task OutboxPublisher_ShouldScheduleFirstPipelineStage()
    {
        PersistenceGraph graph = PersistenceGraphFactory.Create();
        ImportJob requested = ImportJob.Request(
            ImportJobId.New(),
            graph.Source.Id,
            graph.Workspace.Id,
            graph.ImportJob.RequestedAtUtc,
            "publisher-request",
            "sha256:publisher-request",
            "{}").ShouldSucceed();
        graph.Workspace.DequeueDomainEvents();
        graph.Source.DequeueDomainEvents();
        await using (EspadaDbContext setupContext = Fixture.CreateDbContext())
        {
            setupContext.AddRange(graph.Workspace, graph.Source, requested);
            await setupContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();
        using IServiceScope scope = serviceProvider.CreateScope();
        IOutboxPublisher publisher = scope.ServiceProvider.GetRequiredService<IOutboxPublisher>();

        bool published = await publisher.PublishNextAsync(
            "publisher-1",
            TestContext.Current.CancellationToken);

        Assert.True(published);
        IngestionJob? job = await scope.ServiceProvider.GetRequiredService<IJobQueue>()
            .ClaimAsync("worker-1", TimeSpan.FromMinutes(1), TestContext.Current.CancellationToken);
        Assert.NotNull(job);
        Assert.Equal(ImportPipelineStageType.Start, job.Stage);
        Assert.Equal(requested.Id, job.ImportJobId);
    }

    [Fact]
    public async Task TypedSourceDefinition_ShouldRoundTripThroughPostgreSql()
    {
        PersistenceGraph graph = PersistenceGraphFactory.Create();
        Source source = Source.Create(
            SourceId.New(),
            graph.Workspace.Id,
            SourceName.Create("Typed source").ShouldSucceed(),
            new WebPageSourceDefinition(new Uri("https://example.com/typed")),
            DateTimeOffset.UtcNow).ShouldSucceed();
        await using (EspadaDbContext setupContext = Fixture.CreateDbContext())
        {
            setupContext.AddRange(graph.Workspace, source);
            await setupContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();
        using IServiceScope scope = serviceProvider.CreateScope();
        await using Espada.Db.Database.SetupDbContext readContext =
            Fixture.CreateSetupDbContext();
        string json = await readContext.Sources
            .Where(model => model.SourceId == source.Id.Value)
            .Select(model => model.DefinitionJson!)
            .SingleAsync(TestContext.Current.CancellationToken);
        Assert.Contains("\"type\": \"webPage\"", json, StringComparison.Ordinal);

        Source? loaded = await scope.ServiceProvider
            .GetRequiredService<ISourceRepository>()
            .GetByIdAsync(source.Id, TestContext.Current.CancellationToken);
        Assert.IsType<WebPageSourceDefinition>(loaded?.Definition);
    }
}