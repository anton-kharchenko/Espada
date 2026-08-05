using Espada.Cli.Constants;
using Espada.Cli.Infrastructure;
using Espada.Cli.Models;
using System.CommandLine;
using System.Text.Json;

namespace Espada.Cli.Commands.Authentication
{
    internal static class LoginCommand
    {
        public static Command Create(Option<bool> jsonOption)
        {
            Option<bool> noOpenOption = new("--no-open") { Description = "Print the authorization URL only." };
            Command command = new("login", "Sign in to optional Espada Cloud with browser PKCE.");
            command.Options.Add(noOpenOption);
            command.SetAction(async (parseResult, cancellationToken) =>
            {
                try
                {
                    CliHttpResult result = await new LocalApiClient(new LocalRuntimeClient())
                        .SendAsync(HttpMethod.Post, "/api/v1.0/auth/login", new { }, null, cancellationToken);
                    if (!result.IsSuccess)
                    {
                        return CliHttpOutput.Write(result, parseResult.GetValue(jsonOption));
                    }

                    using JsonDocument document = JsonDocument.Parse(result.Content);
                    string authorizationUrl = document.RootElement.GetProperty("authorizationUrl").GetString()
                        ?? throw new InvalidOperationException("Espada returned an invalid authorization URL.");
                    if (parseResult.GetValue(jsonOption))
                    {
                        CliJson.Write(new { authorizationUrl });
                    }
                    else
                    {
                        Console.WriteLine(authorizationUrl);
                    }

                    if (!parseResult.GetValue(noOpenOption) && !BrowserLauncher.TryOpen(authorizationUrl))
                    {
                        await Console.Error.WriteLineAsync("The browser could not be opened. Use the printed authorization URL.");
                    }

                    return CliExitCodesConstants.Success;
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