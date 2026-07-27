using Espada.Cli.Daemon;
using Espada.Cli.Extensions;
using Espada.Comms.Core.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;

RootCommand rootCommand = new("Espada command-line interface");
Command mcpCommand = new("mcp", "Run Model Context Protocol transports.");
Command stdioCommand = new("stdio", "Proxy MCP stdio requests to the local Espada daemon.");

stdioCommand.SetAction(RunMcpStdioAsync);
mcpCommand.Subcommands.Add(stdioCommand);
rootCommand.Subcommands.Add(mcpCommand);

return rootCommand.Parse(args).Invoke();

static async Task<int> RunMcpStdioAsync(ParseResult _, CancellationToken cancellationToken)
{
    string apiKey = Environment.GetEnvironmentVariable(ApiKeyAuthenticationDefaults.EnvironmentVariable) ?? string.Empty;

    if (string.IsNullOrWhiteSpace(apiKey))
    {
        await Console.Error.WriteLineAsync($"{ApiKeyAuthenticationDefaults.EnvironmentVariable} must be set before starting the MCP stdio bridge.");
        return 2;
    }

    string daemonUrl = Environment.GetEnvironmentVariable("ESPADA_DAEMON_URL") ?? "http://127.0.0.1:7432";

    if (!Uri.TryCreate(daemonUrl, UriKind.Absolute, out Uri? baseUri))
    {
        await Console.Error.WriteLineAsync("ESPADA_DAEMON_URL must be an absolute URI.");
        return 2;
    }

    HostApplicationBuilder builder = Host.CreateApplicationBuilder();
    builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
    builder.Services.AddEspadaMcpStdioBridge(new DaemonConnection(baseUri, apiKey));

    await builder.Build().RunAsync(cancellationToken);
    return 0;
}