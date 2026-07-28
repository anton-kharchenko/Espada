using Espada.Db.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Espada.Db.Database
{
    public sealed class SetupDbContextFactory : IDesignTimeDbContextFactory<SetupDbContext>
    {
        public SetupDbContext CreateDbContext(string[] args)
        {
            string connectionString =
                Environment.GetEnvironmentVariable(DbConstants.ConnectionStringEnvironmentVariable) ??
                throw new InvalidOperationException("Connection strings cannot be null");
            DbContextOptionsBuilder<SetupDbContext> options = new();

            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(SetupDbContext).Assembly.FullName);
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", DbConstants.SchemaName);
                npgsql.UseVector();
            });

            return new SetupDbContext(options.Options);
        }
    }
}