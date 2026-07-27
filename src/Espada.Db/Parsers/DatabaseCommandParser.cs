using Espada.Db.Enums;

namespace Espada.Db.Parsers;

internal static class DatabaseCommandParser
{
    public static bool TryParse(string? value, out DatabaseCommandType command)
    {
        string normalized = value?.Trim().ToLowerInvariant() ?? "migrate";

        command = normalized switch
        {
            "migrate" => DatabaseCommandType.Migrate,
            "seed" => DatabaseCommandType.Seed,
            "reset" => DatabaseCommandType.Reset,
            "status" => DatabaseCommandType.Status,
            "help" or "--help" or "-h" => DatabaseCommandType.Help,
            _ => default
        };

        return normalized is
            "migrate" or
            "seed" or
            "reset" or
            "status" or
            "help" or
            "--help" or
            "-h";
    }
}