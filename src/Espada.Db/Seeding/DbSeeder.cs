using Espada.Infrastructure.Database;

namespace Espada.Db.Seeding;

internal static partial class DbSeeder
{
    public static Task SeedAsync(EspadaDbContext dbContext, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        return Task.CompletedTask;
    }
}