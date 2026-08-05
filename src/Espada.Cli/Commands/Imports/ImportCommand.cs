using Espada.Cli.Constants;
using Espada.Cli.Infrastructure;
using Espada.Cli.Models;
using System.CommandLine;
using System.Text.Json;

namespace Espada.Cli.Commands.Imports
{
    internal static class ImportCommand
    {
        public static Command Create(Option<bool> jsonOption)
        {
            Option<Guid> workspaceOption = new("--workspace-id") { Description = "Workspace ID." };
            Option<Guid> sourceOption = new("--source-id") { Description = "Source ID." };
            Option<string?> idempotencyOption = new("--idempotency-key") { Description = "Stable retry key." };
            Option<bool> waitOption = new("--wait") { Description = "Wait for terminal import status." };
            Command command = new("import", "Queue a source import through the local API.");
            command.Options.Add(workspaceOption);
            command.Options.Add(sourceOption);
            command.Options.Add(idempotencyOption);
            command.Options.Add(waitOption);
            command.SetAction(async (parseResult, cancellationToken) =>
            {
                Guid workspaceId = parseResult.GetValue(workspaceOption);
                Guid sourceId = parseResult.GetValue(sourceOption);
                if (workspaceId == Guid.Empty || sourceId == Guid.Empty)
                {
                    await Console.Error.WriteLineAsync("--workspace-id and --source-id are required.");
                    return CliExitCodesConstants.InvalidInput;
                }

                try
                {
                    LocalApiClient api = new(new LocalRuntimeClient());
                    string idempotencyKey = parseResult.GetValue(idempotencyOption) ?? Guid.NewGuid().ToString("N");
                    CliHttpResult result = await api.SendAsync(HttpMethod.Post,
                        $"/api/v1.0/workspaces/{workspaceId:D}/imports", new { sourceId }, idempotencyKey,
                        cancellationToken);
                    if (!result.IsSuccess || !parseResult.GetValue(waitOption))
                    {
                        return CliHttpOutput.Write(result, parseResult.GetValue(jsonOption));
                    }

                    using JsonDocument document = JsonDocument.Parse(result.Content);
                    Guid importJobId = document.RootElement.GetProperty("importJobId").GetGuid();
                    while (true)
                    {
                        CliHttpResult status = await api.SendAsync(HttpMethod.Get,
                            $"/api/v1.0/workspaces/{workspaceId:D}/imports/{importJobId:D}", null, null,
                            cancellationToken);
                        if (!status.IsSuccess)
                        {
                            return CliHttpOutput.Write(status, parseResult.GetValue(jsonOption));
                        }

                        using JsonDocument statusDocument = JsonDocument.Parse(status.Content);
                        if (statusDocument.RootElement.GetProperty("isTerminal").GetBoolean())
                        {
                            return CliHttpOutput.Write(status, parseResult.GetValue(jsonOption));
                        }

                        await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
                    }
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
