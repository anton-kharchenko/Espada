using Espada.Cli.Commands.Authentication;
using Espada.Cli.Commands.Imports;
using Espada.Cli.Commands.Mcp;
using Espada.Cli.Commands.Runtime;
using Espada.Cli.Commands.Search;
using Espada.Cli.Commands.Sources;
using Espada.Cli.Commands.Sync;
using Espada.Cli.Commands.Workspaces;
using System.CommandLine;

namespace Espada.Cli
{
    internal static class CliApplication
    {
        public static RootCommand Create()
        {
            Option<bool> jsonOption = new("--json")
            {
                Description = "Write machine-readable JSON output.",
                Recursive = true
            };
            RootCommand root = new("Espada command-line interface");
            root.Options.Add(jsonOption);
            root.Subcommands.Add(InitCommand.Create(jsonOption));
            root.Subcommands.Add(StartCommand.Create(jsonOption));
            root.Subcommands.Add(StopCommand.Create(jsonOption));
            root.Subcommands.Add(StatusCommand.Create(jsonOption));
            root.Subcommands.Add(WorkspaceCommand.Create(jsonOption));
            root.Subcommands.Add(SourceCommand.Create(jsonOption));
            root.Subcommands.Add(ImportCommand.Create(jsonOption));
            root.Subcommands.Add(SearchCommand.Create(jsonOption));
            root.Subcommands.Add(LoginCommand.Create(jsonOption));
            root.Subcommands.Add(SyncCommand.Create(jsonOption));
            root.Subcommands.Add(McpCommand.Create());
            return root;
        }
    }
}