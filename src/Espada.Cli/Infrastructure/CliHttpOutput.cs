using Espada.Cli.Constants;
using Espada.Cli.Models;
using System.Text.Json;

namespace Espada.Cli.Infrastructure
{
    internal static class CliHttpOutput
    {
        public static int Write(CliHttpResult result, bool json)
        {
            TextWriter writer = result.IsSuccess ? Console.Out : Console.Error;
            if (json || result.IsSuccess)
            {
                writer.WriteLine(FormatJson(result.Content));
            }
            else
            {
                writer.WriteLine(string.IsNullOrWhiteSpace(result.Content)
                    ? $"Espada API returned {result.StatusCode}."
                    : result.Content);
            }

            return result.StatusCode switch
            {
                >= 200 and < 300 => CliExitCodesConstants.Success,
                400 or 422 => CliExitCodesConstants.InvalidInput,
                409 => CliExitCodesConstants.Conflict,
                _ => CliExitCodesConstants.Failure
            };
        }

        private static string FormatJson(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "{}";
            }

            try
            {
                using JsonDocument document = JsonDocument.Parse(value);
                return JsonSerializer.Serialize(document.RootElement, CliJson.Options);
            }
            catch (JsonException)
            {
                return value;
            }
        }
    }
}
