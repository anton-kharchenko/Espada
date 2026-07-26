namespace Espada.Tests.Integration.Fixtures;

public abstract class PostgreSqlIntegrationTest(PostgreSqlDatabaseFixture fixture) : IAsyncLifetime
{
    protected PostgreSqlDatabaseFixture Fixture { get; } = fixture;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await Fixture.ResetDatabaseAsync();
}