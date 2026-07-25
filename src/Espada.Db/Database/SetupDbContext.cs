using Espada.Infrastructure.Database;
using Espada.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Espada.Db.Database;

internal static class SetupDbContext
{
    public static EspadaDbContext Create(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString = Environment.GetEnvironmentVariable("ESPADA_CONNECTION_STRING")
                                  ?? configuration.GetConnectionString("Espada")
                                  ?? throw new InvalidOperationException(
                                      "Database connection string was not configured. Set ConnectionStrings:Espada or ESPADA_CONNECTION_STRING.");

        DbContextOptionsBuilder<EspadaDbContext> optionsBuilder = new();

        optionsBuilder.UseNpgsql(
            connectionString,
            options =>
            {
                options.MigrationsAssembly(typeof(EspadaDbAssemblyMarker).Assembly.FullName);
                options.MigrationsHistoryTable("__EFMigrationsHistory", DbConstants.SchemaName);
            });

        return new EspadaDbContext(optionsBuilder.Options);
    }
}

internal sealed class EspadaDbAssemblyMarker;