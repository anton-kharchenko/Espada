using System.Text.Json;

namespace Espada.Cli.Infrastructure
{
    internal static class CliJson
    {
        public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

        public static void Write(object value)
        {
            Console.WriteLine(JsonSerializer.Serialize(value, Options));
        }
    }
}
