using Espada.Application.Contracts.Embedding;
using Espada.Application.Contracts.Ingestion;
using Espada.Application.Contracts.Jobs;
using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;
using Espada.Domain.ValueObjects.SourceDefinitions;
using Espada.Infrastructure.Database;
using Espada.Tests.Integration.Fixtures;
using Espada.Tests.Integration.Transactions.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json;

namespace Espada.Tests.Integration.Transactions
{
    [Collection(PostgreSqlIntegrationCollection.Name)]
    public sealed class AutomaticIngestionPipelineTests(PostgreSqlDatabaseFixture fixture)
        : PostgreSqlIntegrationTest(fixture)
    {
        [Fact]
        public async Task PlainTextImport_ShouldRunToSearchableSucceededRevision()
        {
            string blobRoot = Path.Join(Path.GetTempPath(), "espada-pipeline-tests", Guid.NewGuid().ToString("N"));
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Ingestion:BlobRoot"] = blobRoot })
                .Build();

            await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider(
                configuration,
                services => services.Replace(
                    ServiceDescriptor.Transient<
                        IBatchEmbeddingGeneratorService,
                        FakeBatchEmbeddingGeneratorService>()));
            try
            {
                WorkspaceId workspaceId = WorkspaceId.New();
                SourceId sourceId = SourceId.New();
                ImportJobId importJobId = ImportJobId.New();
                DateTimeOffset now = DateTimeOffset.UtcNow;
                Workspace workspace = Workspace.Create(workspaceId, WorkspaceName.Create("Pipeline test").Value,
                    WorkspaceType.Personal, null, now).Value;
                Source source = Source.Create(
                    sourceId,
                    workspaceId,
                    SourceName.Create("Stage 10").Value,
                    new PlainTextSourceDefinition(
                        "Stage 10",
                        "Espada automatically creates searchable context from registered sources."),
                    now).Value;
                ImportOptions options = new("test-model@v1");
                ImportJob importJob = ImportJob.Request(
                    importJobId,
                    sourceId,
                    workspaceId,
                    now,
                    "automatic-pipeline",
                    "automatic-pipeline-fingerprint",
                    JsonSerializer.Serialize(
                        options,
                        new JsonSerializerOptions(JsonSerializerDefaults.Web))).Value;

                using (IServiceScope setupScope = serviceProvider.CreateScope())
                {
                    EspadaDbContext dbContext = setupScope.ServiceProvider.GetRequiredService<EspadaDbContext>();
                    dbContext.AddRange(workspace, source, importJob);
                    await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
                }

                for (int completedStages = 0; completedStages < 7; completedStages++)
                {
                    IngestionJob? job = null;
                    for (int publishAttempts = 0; publishAttempts < 20 && job is null; publishAttempts++)
                    {
                        using IServiceScope publishScope = serviceProvider.CreateScope();
                        await publishScope.ServiceProvider
                            .GetRequiredService<IOutboxPublisher>()
                            .PublishNextAsync(
                                "test-publisher",
                                TestContext.Current.CancellationToken);
                        job = await publishScope.ServiceProvider.GetRequiredService<IJobQueue>()
                            .ClaimAsync("test-worker", TimeSpan.FromMinutes(1), TestContext.Current.CancellationToken);
                    }

                    Assert.NotNull(job);
                    IngestionJob claimedJob = job;
                    using IServiceScope executionScope = serviceProvider.CreateScope();
                    await executionScope.ServiceProvider
                        .GetRequiredService<IImportPipelineStageExecutorService>()
                        .ExecuteAsync(claimedJob, TestContext.Current.CancellationToken);
                    await executionScope.ServiceProvider
                        .GetRequiredService<IJobQueue>()
                        .CompleteAsync(
                            claimedJob.JobId,
                            "test-worker",
                            TestContext.Current.CancellationToken);
                }

                await using EspadaDbContext verification = Fixture.CreateDbContext();
                ImportJob completed = await verification.ImportJobs.SingleAsync(value => value.Id == importJobId,
                    TestContext.Current.CancellationToken);
                Assert.Equal(ImportStatusType.Succeeded, completed.Status);
                Assert.NotNull(completed.ArtifactId);
                Assert.NotNull(completed.ArtifactRevisionId);
                Assert.Single(await verification.Chunks.ToListAsync(TestContext.Current.CancellationToken));
                Assert.Single(await verification.ChunkEmbeddings.ToListAsync(TestContext.Current.CancellationToken));
            }
            finally
            {
                if (Directory.Exists(blobRoot))
                {
                    Directory.Delete(blobRoot, true);
                }
            }
        }
    }
}