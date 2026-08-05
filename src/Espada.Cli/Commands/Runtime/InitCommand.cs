using Espada.Cli.Constants;
using Espada.Cli.Infrastructure;
using Espada.Cli.Models;
using System.CommandLine;

namespace Espada.Cli.Commands.Runtime
{
    internal static class InitCommand
    {
        public static Command Create(Option<bool> jsonOption)
        {
            Argument<string?> pathArgument = new("path")
            {
                Description = "Repository path. Defaults to the current directory."
            };
            Option<bool> noOpenOption = new("--no-open")
            {
                Description = "Print the one-time setup URL without opening a browser."
            };
            Command command = new("init", "Start Espada and open the UI setup wizard.");
            command.Arguments.Add(pathArgument);
            command.Options.Add(noOpenOption);
            command.SetAction(async (parseResult, cancellationToken) =>
            {
                string path = parseResult.GetValue(pathArgument) ?? Directory.GetCurrentDirectory();
                if (!Directory.Exists(path))
                {
                    await Console.Error.WriteLineAsync($"Repository path does not exist: {path}");
                    return CliExitCodesConstants.InvalidInput;
                }

                try
                {
                    LocalRuntimeClient runtime = new();
                    await runtime.StartAsync(cancellationToken);
                    BootstrapLinkResponse link = await new LocalApiClient(runtime)
                        .CreateSetupLinkAsync(path, cancellationToken);
                    if (parseResult.GetValue(jsonOption))
                    {
                        CliJson.Write(new { setupUrl = link.Url, link.ExpiresInSeconds });
                    }
                    else
                    {
                        Console.WriteLine(link.Url);
                    }

                    if (!parseResult.GetValue(noOpenOption) && !BrowserLauncher.TryOpen(link.Url))
                    {
                        await Console.Error.WriteLineAsync("The browser could not be opened. Use the printed setup URL.");
                    }

                    return CliExitCodesConstants.Success;
                }
                catch (Exception exception)
                {
                    await Console.Error.WriteLineAsync(exception.Message);
                    return CliExitCodesConstants.Failure;
                }
            });
            return command;
        }
    }
}
