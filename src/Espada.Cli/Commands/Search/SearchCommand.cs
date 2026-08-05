using Espada.Cli.Constants;
using Espada.Cli.Infrastructure;
using Espada.Cli.Models;
using System.CommandLine;

namespace Espada.Cli.Commands.Search
{
    internal static class SearchCommand
    {
        public static Command Create(Option<bool> jsonOption)
        {
            Argument<string> queryArgument = new("query") { Description = "Search query." };
            Option<Guid> workspaceOption = new("--workspace-id") { Description = "Workspace ID." };
            Option<int> limitOption = new("--limit") { Description = "Maximum hits.", DefaultValueFactory = _ => 20 };
            Command command = new("search", "Search unified local context.");
            command.Arguments.Add(queryArgument);
            command.Options.Add(workspaceOption);
            command.Options.Add(limitOption);
            command.SetAction(async (parseResult, cancellationToken) =>
            {
                Guid workspaceId = parseResult.GetValue(workspaceOption);
                string? query = parseResult.GetValue(queryArgument);
                if (workspaceId == Guid.Empty || string.IsNullOrWhiteSpace(query))
                {
                    await Console.Error.WriteLineAsync("query and --workspace-id are required.");
                    return CliExitCodesConstants.InvalidInput;
                }

                try
                {
                    string path = $"/api/v1.0/workspaces/{workspaceId:D}/search?query={Uri.EscapeDataString(query)}"
                        + $"&limit={parseResult.GetValue(limitOption)}";
                    CliHttpResult result = await new LocalApiClient(new LocalRuntimeClient())
                        .SendAsync(HttpMethod.Get, path, null, null, cancellationToken);
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