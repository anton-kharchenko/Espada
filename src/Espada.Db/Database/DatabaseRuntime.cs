using Npgsql;

namespace Espada.Db.Database;

internal sealed class DatabaseRuntime(SetupDbContext dbContext, NpgsqlDataSource dataSource) : IAsyncDisposable
{
    public SetupDbContext DbContext { get; } = dbContext;

    public async ValueTask DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await dataSource.DisposeAsync();
    }
}