using Espada.Cli.Constants;
using Espada.Cli.Infrastructure;
using Espada.Cli.Models;
using System.CommandLine;

namespace Espada.Cli.Commands.Sync
{
    internal static class SyncCommand
    {
        public static Command Create(Option<bool> jsonOption)
        {
            Command command = new("sync", "Run an immediate sync push/pull cycle.");
            command.SetAction(async (parseResult, cancellationToken) =>
            {
                try
                {
                    CliHttpResult result = await new LocalApiClient(new LocalRuntimeClient())
                        .SendAsync(HttpMethod.Post, "/api/v1.0/sync", new { }, null, cancellationToken);
                    return CliHttpOutput.Write(result, parseResult.GetValue(jsonOption));
                }
                catch (HttpRequestException exception)
                {
                    await Console.Error.WriteLineAsync(exception.Message);
                    return CliExitCodesConstants.DaemonUnavailable;
                }
            });
            return command;
        }
    }
}
