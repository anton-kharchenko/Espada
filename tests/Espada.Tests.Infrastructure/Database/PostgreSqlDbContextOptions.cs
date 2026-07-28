using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Espada.Tests.Infrastructure.Database
{
    internal static class PostgreSqlDbContextOptions
    {
        public static DbContextOptions<TContext> Create<TContext>(string connectionString,
            Action<NpgsqlDbContextOptionsBuilder>? configure = null) where TContext : DbContext
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            DbContextOptionsBuilder<TContext> options = new();
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.UseVector();
                configure?.Invoke(npgsql);
            });

            return options.Options;
        }
    }
}