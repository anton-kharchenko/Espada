using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.Common;

namespace Espada.Tests.Common.Database;

public static class PostgreSqlDatabaseCleaner
{
    public static async Task ResetAsync(DbContext dbContext, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        ISqlGenerationHelper sql = dbContext.GetService<ISqlGenerationHelper>();
        string[] tables = dbContext.Model.GetEntityTypes()
            .Select(entityType => (Schema: entityType.GetSchema(), Table: entityType.GetTableName()))
            .Where(mapping => mapping.Table is not null)
            .Distinct()
            .Select(mapping => sql.DelimitIdentifier(mapping.Table!, mapping.Schema))
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (tables.Length == 0)
        {
            return;
        }

        await dbContext.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            await using DbCommand command = dbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"TRUNCATE TABLE {string.Join(", ", tables)} RESTART IDENTITY CASCADE;";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            await dbContext.Database.CloseConnectionAsync();
        }
    }
}