using Espada.Application.UseCases.Memories.Commands.RememberMemory;
using Espada.Application.UseCases.Memories.Queries.SearchMemory;
using Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;
using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Espada.Tests.Integration.Fixtures;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Espada.Tests.Integration.Repositories
{
    [Collection(PostgreSqlIntegrationCollection.Name)]
    public sealed class MemorySearchRepositoryTests(
        PostgreSqlDatabaseFixture fixture) : PostgreSqlIntegrationTest(fixture)
    {
        [Fact]
        public async Task Search_ShouldReturnCurrentMemoryAndExcludeSupersededMemory()
        {
            await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();
            using IServiceScope scope = serviceProvider.CreateScope();
            IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            CancellationToken cancellationToken = TestContext.Current.CancellationToken;

            DomainResult<CreateWorkspaceResponse> workspaceResult = await mediator.Send(
                new CreateWorkspaceCommand("Memory workspace", WorkspaceType.Personal),
                cancellationToken);
            Guid workspaceId = workspaceResult.Value.WorkspaceId;

            DomainResult<RememberMemoryResponse> firstResult = await mediator.Send(
                new RememberMemoryCommand(
                    workspaceId,
                    "Database",
                    "Use SQLite for local persistence.",
                    MemoryCategoryType.Decision.Id,
                    0.7m,
                    "codex",
                    "session-1"),
                cancellationToken);
            DomainResult<RememberMemoryResponse> secondResult = await mediator.Send(
                new RememberMemoryCommand(
                    workspaceId,
                    "Database",
                    "Use PostgreSQL for local persistence.",
                    MemoryCategoryType.Decision.Id,
                    0.9m,
                    "codex",
                    "session-2",
                    firstResult.Value.MemoryId),
                cancellationToken);

            DomainResult<SearchMemoryResponse> searchResult = await mediator.Send(
                new SearchMemoryQuery(
                    workspaceId,
                    "PostgreSQL persistence",
                    [MemoryCategoryType.Decision.Id]),
                cancellationToken);

            SearchMemoryResponse response = searchResult.Value;
            MemorySearchItemResponse item = Assert.Single(response.Items);
            Assert.Equal(secondResult.Value.MemoryId, item.MemoryId);
            Assert.Equal("Use PostgreSQL for local persistence.", item.Content);
            Assert.False(item.Provenance.UserConfirmed);
            Assert.Equal("codex", item.Provenance.ClientIdentity);
            Assert.Equal("session-2", item.Provenance.SessionIdentity);
            Assert.Equal(firstResult.Value.MemoryId, item.Provenance.SupersededMemoryId);
        }

        [Fact]
        public async Task Search_WithMultipleChunks_ShouldApplyTopKToDistinctMemories()
        {
            await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();
            using IServiceScope scope = serviceProvider.CreateScope();
            IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            CancellationToken cancellationToken = TestContext.Current.CancellationToken;
            Guid workspaceId = (await mediator.Send(
                new CreateWorkspaceCommand(
                    "Distinct memory search",
                    WorkspaceType.Personal),
                cancellationToken)).Value.WorkspaceId;
            RememberMemoryResponse first = (await mediator.Send(
                new RememberMemoryCommand(
                    workspaceId,
                    "First",
                    "chunkcollapse first",
                    MemoryCategoryType.Fact.Id,
                    0.8m,
                    "integration-test"),
                cancellationToken)).Value;
            RememberMemoryResponse second = (await mediator.Send(
                new RememberMemoryCommand(
                    workspaceId,
                    "Second",
                    "chunkcollapse second",
                    MemoryCategoryType.Fact.Id,
                    0.8m,
                    "integration-test"),
                cancellationToken)).Value;
            EspadaDbContext dbContext = scope.ServiceProvider.GetRequiredService<EspadaDbContext>();
            ArtifactId firstArtifactId = ArtifactId.Create(first.ArtifactId);
            Chunk firstChunk = await dbContext.Chunks.SingleAsync(
                chunk => chunk.ArtifactId == firstArtifactId,
                cancellationToken);
            for (int index = 1; index <= 3; index++)
            {
                Chunk extraChunk = Chunk.Create(
                    ChunkId.Create(Guid.NewGuid()),
                    firstChunk.BatchId,
                    firstChunk.WorkspaceId,
                    firstChunk.ArtifactId,
                    firstChunk.ArtifactRevisionId,
                    ChunkNumber.Create(firstChunk.Number.Value + index).Value,
                    ChunkContent.Create(string.Join(
                        " ",
                        Enumerable.Repeat("chunkcollapse", 40))).Value,
                    null,
                    firstChunk.Strategy,
                    firstChunk.StrategyVersion,
                    firstChunk.CreatedAtUtc.AddSeconds(index)).Value;
                dbContext.Chunks.Add(extraChunk);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            SearchMemoryResponse response = (await mediator.Send(
                new SearchMemoryQuery(workspaceId, "chunkcollapse", [], 2),
                cancellationToken)).Value;

            Assert.Equal(2, response.Items.Count);
            Assert.Equal(2, response.Items.Select(item => item.MemoryId).Distinct().Count());
            Assert.Contains(first.MemoryId, response.Items.Select(item => item.MemoryId));
            Assert.Contains(second.MemoryId, response.Items.Select(item => item.MemoryId));
        }

        [Fact]
        public async Task Save_WithTwoSuccessorsForOneMemory_ShouldRejectSecondSuccessor()
        {
            await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();
            using IServiceScope scope = serviceProvider.CreateScope();
            IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            CancellationToken cancellationToken = TestContext.Current.CancellationToken;
            Guid workspaceId = (await mediator.Send(
                new CreateWorkspaceCommand(
                    "Memory supersession constraint",
                    WorkspaceType.Personal),
                cancellationToken)).Value.WorkspaceId;
            RememberMemoryResponse original = await RememberAsync(
                mediator,
                workspaceId,
                "Original",
                cancellationToken);
            RememberMemoryResponse firstSuccessor = await RememberAsync(
                mediator,
                workspaceId,
                "First successor",
                cancellationToken);
            RememberMemoryResponse secondSuccessor = await RememberAsync(
                mediator,
                workspaceId,
                "Second successor",
                cancellationToken);
            EspadaDbContext dbContext = scope.ServiceProvider.GetRequiredService<EspadaDbContext>();

            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE \"Espada\".\"MemoryMetadata\" SET \"SupersededMemoryId\" = {original.MemoryId} WHERE \"MemoryId\" = {firstSuccessor.MemoryId}",
                cancellationToken);
            PostgresException exception = await Assert.ThrowsAsync<PostgresException>(() =>
                dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE \"Espada\".\"MemoryMetadata\" SET \"SupersededMemoryId\" = {original.MemoryId} WHERE \"MemoryId\" = {secondSuccessor.MemoryId}",
                    cancellationToken));

            Assert.Equal(PostgresErrorCodes.UniqueViolation, exception.SqlState);
            Assert.Equal(
                DbIndexConstants.MemoryMetadataSupersededMemory,
                exception.ConstraintName);
        }

        private static async Task<RememberMemoryResponse> RememberAsync(
            IMediator mediator,
            Guid workspaceId,
            string title,
            CancellationToken cancellationToken)
        {
            DomainResult<RememberMemoryResponse> result = await mediator.Send(
                new RememberMemoryCommand(
                    workspaceId,
                    title,
                    $"{title} content",
                    MemoryCategoryType.Fact.Id,
                    0.8m,
                    "integration-test"),
                cancellationToken);

            return result.Value;
        }
    }
}