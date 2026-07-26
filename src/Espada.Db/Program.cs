using Espada.Db.Commands;
using Espada.Db.Database;
using Espada.Db.Enums;
using Espada.Db.Extensions;
using Espada.Db.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Espada.Db;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        IConfiguration configuration = DbConfiguration.Create();

        await using DatabaseRuntime databaseRuntime = SetupDbContext.CreateRuntime(configuration);
        SetupDbContext dbContext = databaseRuntime.DbContext;
        string? commandValue = args.FirstOrDefault();

        if (!DatabaseCommandParser.TryParse(commandValue, out DatabaseCommandType command))
        {
            return PrintUnknownCommand(commandValue ?? string.Empty);
        }

        bool forceRequested = args.Any(argument => argument.Equals("--force", StringComparison.OrdinalIgnoreCase));

        if (!command.RequiresForce() || forceRequested)
        {
            return command switch
            {
                DatabaseCommandType.Migrate => await MigrateAsync(dbContext),
                DatabaseCommandType.Seed => await SeedAsync(dbContext),
                DatabaseCommandType.Reset => await ResetAsync(dbContext),
                DatabaseCommandType.Status => await PrintStatusAsync(dbContext),
                DatabaseCommandType.Help => PrintHelp(),
                _ => throw new ArgumentOutOfRangeException(nameof(command), command, null)
            };
        }

        await Console.Error.WriteLineAsync($"The '{command.ToString().ToLowerInvariant()}' command requires --force.");

        return 2;
    }

    private static async Task<int> MigrateAsync(SetupDbContext dbContext)
    {
        Console.WriteLine("Applying database migrations...");

        await dbContext.Database.MigrateAsync();
        await DbSeeder.SeedAsync(dbContext);

        Console.WriteLine("Database migrations and reference data seeding completed.");

        return 0;
    }


    private static async Task<int> SeedAsync(SetupDbContext dbContext)
    {
        Console.WriteLine("Applying database migrations...");

        await dbContext.Database.MigrateAsync();

        Console.WriteLine("Seeding database...");

        await DbSeeder.SeedAsync(dbContext);

        Console.WriteLine("Database seeding completed.");

        return 0;
    }

    private static async Task<int> ResetAsync(SetupDbContext dbContext)
    {
        Console.WriteLine("Deleting database...");

        await dbContext.Database.EnsureDeletedAsync();

        Console.WriteLine("Applying migrations...");

        await dbContext.Database.MigrateAsync();

        Console.WriteLine("Seeding database...");

        await DbSeeder.SeedAsync(dbContext);

        Console.WriteLine("Database reset completed.");

        return 0;
    }

    private static async Task<int> PrintStatusAsync(SetupDbContext dbContext)
    {
        bool canConnect = await dbContext.Database.CanConnectAsync();

        Console.WriteLine($"Can connect: {canConnect}");

        IEnumerable<string> applied = await dbContext.Database.GetAppliedMigrationsAsync();

        IEnumerable<string> pending = await dbContext.Database.GetPendingMigrationsAsync();

        Console.WriteLine();
        Console.WriteLine("Applied migrations:");

        foreach (string migration in applied)
        {
            Console.WriteLine($"  {migration}");
        }

        Console.WriteLine();
        Console.WriteLine("Pending migrations:");

        foreach (string migration in pending)
        {
            Console.WriteLine($"  {migration}");
        }

        return canConnect ? 0 : 1;
    }

    private static int PrintHelp()
    {
        Console.WriteLine(
            """
            Espada database utility

            Commands:
              migrate          Apply pending migrations
              seed             Apply migrations and seed data
              status           Show database and migration status
              reset --force    Delete, recreate and seed the database
              help             Show this help
            """);

        return 0;
    }

    private static int PrintUnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown database command: {command}");

        PrintHelp();

        return 2;
    }
}