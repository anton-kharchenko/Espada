using Espada.Cli.Constants;
using Espada.Cli.Infrastructure;
using Espada.Cli.Models;
using System.CommandLine;
using System.Text.Json;

namespace Espada.Cli.Commands.Sources
{
    internal static class SourceCommand
    {
        public static Command Create(Option<bool> jsonOption)
        {
            Option<Guid> workspaceOption = new("--workspace-id") { Description = "Workspace ID." };
            Option<string> nameOption = new("--name") { Description = "Source name." };
            Option<string> typeOption = new("--type") { Description = "repository, file, web-page, plain-text, conversation, or connector." };
            Option<string?> valueOption = new("--value") { Description = "Path, URL, text, or repository root." };
            Option<string?> titleOption = new("--title") { Description = "Plain-text or conversation title." };
            Option<string?> mediaTypeOption = new("--media-type") { Description = "File media type." };
            Option<string?> remoteOption = new("--remote") { Description = "Canonical repository remote URI." };
            Option<Guid> projectOption = new("--project-id") { Description = "Repository project ID." };
            Option<string?> jsonDefinitionOption = new("--definition-json")
            {
                Description = "Typed source definition JSON for conversation or connector sources."
            };
            Command add = new("add", "Register a typed source through the local API.");
            add.Options.Add(workspaceOption);
            add.Options.Add(nameOption);
            add.Options.Add(typeOption);
            add.Options.Add(valueOption);
            add.Options.Add(titleOption);
            add.Options.Add(mediaTypeOption);
            add.Options.Add(remoteOption);
            add.Options.Add(projectOption);
            add.Options.Add(jsonDefinitionOption);
            add.SetAction(async (parseResult, cancellationToken) =>
            {
                Guid workspaceId = parseResult.GetValue(workspaceOption);
                string? name = parseResult.GetValue(nameOption);
                string? type = parseResult.GetValue(typeOption);
                if (workspaceId == Guid.Empty || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(type))
                {
                    await Console.Error.WriteLineAsync("--workspace-id, --name, and --type are required.");
                    return CliExitCodesConstants.InvalidInput;
                }

                object? definition;
                try
                {
                    definition = CreateDefinition(type, parseResult.GetValue(valueOption),
                        parseResult.GetValue(titleOption), parseResult.GetValue(mediaTypeOption),
                        parseResult.GetValue(remoteOption), parseResult.GetValue(projectOption),
                        parseResult.GetValue(jsonDefinitionOption));
                }
                catch (Exception exception) when (exception is ArgumentException or JsonException)
                {
                    await Console.Error.WriteLineAsync(exception.Message);
                    return CliExitCodesConstants.InvalidInput;
                }

                try
                {
                    LocalRuntimeClient runtime = new();
                    CliHttpResult result = await new LocalApiClient(runtime).SendAsync(HttpMethod.Post,
                        $"/api/v1.0/workspaces/{workspaceId:D}/sources", new { name, definition }, null,
                        cancellationToken);
                    return CliHttpOutput.Write(result, parseResult.GetValue(jsonOption));
                }
                catch (HttpRequestException exception)
                {
                    await Console.Error.WriteLineAsync(exception.Message);
                    return CliExitCodesConstants.DaemonUnavailable;
                }
            });

            Command source = new("source", "Manage typed sources.");
            source.Subcommands.Add(add);
            return source;
        }

        private static object CreateDefinition(string type, string? value, string? title, string? mediaType,
            string? remote, Guid projectId, string? definitionJson)
        {
            if (!string.IsNullOrWhiteSpace(definitionJson))
            {
                return JsonSerializer.Deserialize<JsonElement>(definitionJson);
            }

            return type.ToLowerInvariant() switch
            {
                "repository" when projectId != Guid.Empty => new
                {
                    type = "repository",
                    repositoryIdentity = projectId.ToString("D"),
                    canonicalRemoteUri = remote,
                    scanPolicy = new { trackedFilesOnly = true, maximumFileSizeBytes = 5_242_880 }
                },
                "file" when !string.IsNullOrWhiteSpace(value) => new
                {
                    type = "file",
                    localPath = Path.GetFullPath(value),
                    blob = (object?)null,
                    fileName = Path.GetFileName(value),
                    mediaType = mediaType ?? "text/plain"
                },
                "web-page" when Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) => new
                {
                    type = "webPage",
                    uri
                },
                "plain-text" when !string.IsNullOrWhiteSpace(value) => new
                {
                    type = "plainText",
                    title = title ?? "Text",
                    content = value
                },
                "conversation" or "connector" => throw new ArgumentException(
                    "--definition-json is required for conversation and connector sources."),
                _ => throw new ArgumentException(
                    "The source type or required --value/--project-id option is invalid.")
            };
        }
    }
}