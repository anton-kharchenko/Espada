using Espada.Cli.Constants;
using Espada.Cli.Infrastructure;
using Espada.Cli.Models;
using System.CommandLine;

namespace Espada.Cli.Commands.Workspaces
{
    internal static class WorkspaceCommand
    {
        public static Command Create(Option<bool> jsonOption)
        {
            Option<string> nameOption = new("--name") { Description = "Workspace name." };
            Option<int> typeOption = new("--type")
            {
                Description = "Workspace type ID.",
                DefaultValueFactory = _ => 1
            };
            Command create = new("create", "Create a workspace through the local API.");
            create.Options.Add(nameOption);
            create.Options.Add(typeOption);
            create.SetAction(async (parseResult, cancellationToken) =>
            {
                string? name = parseResult.GetValue(nameOption);
                if (string.IsNullOrWhiteSpace(name))
                {
                    await Console.Error.WriteLineAsync("--name is required.");
                    return CliExitCodesConstants.InvalidInput;
                }

                try
                {
                    LocalRuntimeClient runtime = new();
                    CliHttpResult result = await new LocalApiClient(runtime).SendAsync(HttpMethod.Post,
                        "/api/v1.0/workspaces", new { name, typeId = parseResult.GetValue(typeOption) }, null,
                        cancellationToken);
                    return CliHttpOutput.Write(result, parseResult.GetValue(jsonOption));
                }
                catch (HttpRequestException exception)
                {
                    await Console.Error.WriteLineAsync(exception.Message);
                    return CliExitCodesConstants.DaemonUnavailable;
                }
            });

            Command workspace = new("workspace", "Manage workspaces.");
            workspace.Subcommands.Add(create);
            return workspace;
        }
    }
}
