using Espada.Cli.Infrastructure;
using Espada.Mcp;
using System.CommandLine;
using System.Globalization;

namespace Espada.Cli.Commands.Mcp
{
    internal static class McpCommand
    {
        public static Command Create()
        {
            Option<Guid?> workspaceOption = new("--workspace-id") { Description = "Trusted local workspace ID." };
            Option<string?> clientOption = new("--client-id") { Description = "MCP client identity." };
            Option<string?> issuerOption = new("--identity-issuer") { Description = "Trusted local identity issuer." };
            Option<string?> subjectOption = new("--identity-subject") { Description = "Trusted local identity subject." };
            Option<string?> scopesOption = new("--scopes") { Description = "Comma-separated exact MCP scopes." };
            Option<int?> ceilingOption = new("--rate-ceiling") { Description = "Maximum requests per minute." };
            Command stdio = new("stdio", "Run the trusted local Espada MCP stdio transport.");
            stdio.Options.Add(workspaceOption);
            stdio.Options.Add(clientOption);
            stdio.Options.Add(issuerOption);
            stdio.Options.Add(subjectOption);
            stdio.Options.Add(scopesOption);
            stdio.Options.Add(ceilingOption);
            stdio.SetAction(async (parseResult, cancellationToken) =>
            {
                LocalRuntimeClient runtime = new();
                await runtime.StartAsync(cancellationToken);
                List<string> arguments = [];
                Add(arguments, "ConnectionStrings:Espada", runtime.CreateConnectionString());
                Add(arguments, "Mcp:TrustedLocal:WorkspaceId",
                    parseResult.GetValue(workspaceOption)?.ToString("D"));
                Add(arguments, "Mcp:TrustedLocal:ClientId", parseResult.GetValue(clientOption));
                Add(arguments, "Mcp:TrustedLocal:IdentityIssuer", parseResult.GetValue(issuerOption));
                Add(arguments, "Mcp:TrustedLocal:IdentitySubject", parseResult.GetValue(subjectOption));
                Add(arguments, "Mcp:TrustedLocal:Scopes", parseResult.GetValue(scopesOption));
                Add(arguments, "Mcp:TrustedLocal:RateCeilingPerMinute",
                    parseResult.GetValue(ceilingOption)?.ToString(CultureInfo.InvariantCulture));
                return await McpBootstrap.RunStdioAsync(arguments.ToArray(), cancellationToken);
            });

            Command command = new("mcp", "Run Model Context Protocol transports.");
            command.Subcommands.Add(stdio);
            return command;
        }

        private static void Add(ICollection<string> arguments, string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            arguments.Add($"--{key}");
            arguments.Add(value);
        }
    }
}