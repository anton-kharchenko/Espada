using Espada.Mcp;
using Espada.Mcp.Responses;
using System.CommandLine;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;

RootCommand rootCommand = new("Espada command-line interface");
Command mcpCommand = new("mcp", "Run Model Context Protocol transports.");
Command stdioCommand = new(
    "stdio",
    "Run the trusted local Espada MCP stdio transport.");
Option<Guid?> workspaceIdOption = new("--workspace-id")
{
    Description = "Workspace bound to the trusted local MCP principal."
};
Option<string?> clientIdOption = new("--client-id") { Description = "MCP client identity." };
Option<string?> identityIssuerOption = new("--identity-issuer") { Description = "Trusted local identity issuer." };
Option<string?> identitySubjectOption = new("--identity-subject") { Description = "Trusted local identity subject." };
Option<string?> scopesOption = new("--scopes") { Description = "Comma-separated exact MCP scopes." };
Option<int?> rateCeilingOption = new("--rate-ceiling") { Description = "Maximum requests per minute for this client." };

stdioCommand.Options.Add(workspaceIdOption);
stdioCommand.Options.Add(clientIdOption);
stdioCommand.Options.Add(identityIssuerOption);
stdioCommand.Options.Add(identitySubjectOption);
stdioCommand.Options.Add(scopesOption);
stdioCommand.Options.Add(rateCeilingOption);
stdioCommand.SetAction(RunMcpStdioAsync);
mcpCommand.Subcommands.Add(stdioCommand);
rootCommand.Subcommands.Add(mcpCommand);

Command authCommand = new(
    "auth",
    "Authorize local Espada clients.");
Command bootstrapCommand = new(
    "bootstrap",
    "Create a one-time local authorization link.");
Option<string> endpointOption = new("--endpoint")
{
    Description = "Local Espada MCP authority endpoint.",
    DefaultValueFactory = _ => "http://127.0.0.1:7433"
};
Option<string?> returnUrlOption = new("--return-url") { Description = "Local path to open after authorization." };
bootstrapCommand.Options.Add(endpointOption);
bootstrapCommand.Options.Add(returnUrlOption);
bootstrapCommand.SetAction(CreateBootstrapLinkAsync);
authCommand.Subcommands.Add(bootstrapCommand);
rootCommand.Subcommands.Add(authCommand);

Command consoleCommand = new(
    "console",
    "Open a one-time local Espada Web Console session.");
Option<string> consoleEndpointOption = new("--endpoint")
{
    Description = "Local Espada Web Console endpoint.",
    DefaultValueFactory = _ => "http://127.0.0.1:5173"
};
Option<bool> noOpenOption = new("--no-open")
{
    Description = "Print the one-time link without opening the browser."
};
consoleCommand.Options.Add(consoleEndpointOption);
consoleCommand.Options.Add(noOpenOption);
consoleCommand.SetAction(OpenConsoleAsync);
rootCommand.Subcommands.Add(consoleCommand);

return rootCommand.Parse(args).Invoke();

async Task<int> RunMcpStdioAsync(
    ParseResult parseResult,
    CancellationToken cancellationToken)
{
    List<string> hostArguments = [];
    AddConfigurationArgument(
        hostArguments,
        "Mcp:TrustedLocal:WorkspaceId",
        parseResult.GetValue(workspaceIdOption)?.ToString("D"));
    AddConfigurationArgument(
        hostArguments,
        "Mcp:TrustedLocal:ClientId",
        parseResult.GetValue(clientIdOption));
    AddConfigurationArgument(
        hostArguments,
        "Mcp:TrustedLocal:IdentityIssuer",
        parseResult.GetValue(identityIssuerOption));
    AddConfigurationArgument(
        hostArguments,
        "Mcp:TrustedLocal:IdentitySubject",
        parseResult.GetValue(identitySubjectOption));
    AddConfigurationArgument(
        hostArguments,
        "Mcp:TrustedLocal:Scopes",
        parseResult.GetValue(scopesOption));
    AddConfigurationArgument(
        hostArguments,
        "Mcp:TrustedLocal:RateCeilingPerMinute",
        parseResult.GetValue(rateCeilingOption)?.ToString(
            CultureInfo.InvariantCulture));

    return await McpBootstrap.RunStdioAsync(
        hostArguments.ToArray(),
        cancellationToken);
}

static void AddConfigurationArgument(
    ICollection<string> arguments,
    string key,
    string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return;
    }

    arguments.Add($"--{key}");
    arguments.Add(value);
}

async Task<int> CreateBootstrapLinkAsync(
    ParseResult parseResult,
    CancellationToken cancellationToken)
{
    string endpointValue = parseResult.GetRequiredValue(endpointOption);
    if (!Uri.TryCreate(endpointValue, UriKind.Absolute, out Uri? endpoint)
        || !IsLocalAuthority(endpoint))
    {
        Console.Error.WriteLine(
            "The bootstrap endpoint must use a loopback HTTP address.");
        return 1;
    }

    string? returnUrl = parseResult.GetValue(returnUrlOption);
    BootstrapLinkResponse? link = await RequestBootstrapLinkAsync(
        endpoint,
        "/auth/bootstrap-links",
        returnUrl,
        cancellationToken);
    if (link is null)
    {
        return 1;
    }

    Console.WriteLine(link.Url);
    return 0;
}

async Task<int> OpenConsoleAsync(
    ParseResult parseResult,
    CancellationToken cancellationToken)
{
    string endpointValue = parseResult.GetRequiredValue(
        consoleEndpointOption);
    if (!Uri.TryCreate(endpointValue, UriKind.Absolute, out Uri? endpoint)
        || !IsLocalAuthority(endpoint))
    {
        Console.Error.WriteLine(
            "The Web Console endpoint must use a loopback HTTP address.");
        return 1;
    }

    BootstrapLinkResponse? link = await RequestBootstrapLinkAsync(
        endpoint,
        "/bff/auth/bootstrap-link",
        "/app",
        cancellationToken);
    if (link is null)
    {
        return 1;
    }

    Console.WriteLine(link.Url);
    if (!parseResult.GetValue(noOpenOption))
    {
        try
        {
            Process.Start(
                new ProcessStartInfo(link.Url)
                {
                    UseShellExecute = true
                });
        }
        catch (Exception exception)
            when (exception is InvalidOperationException
                  or System.ComponentModel.Win32Exception)
        {
            Console.Error.WriteLine(
                "The browser could not be opened. Use the printed link.");
        }
    }

    return 0;
}

static async Task<BootstrapLinkResponse?> RequestBootstrapLinkAsync(
    Uri endpoint,
    string path,
    string? returnUrl,
    CancellationToken cancellationToken)
{
    UriBuilder requestUri = new(new Uri(endpoint, path));
    if (!string.IsNullOrWhiteSpace(returnUrl))
    {
        requestUri.Query =
            $"returnUrl={Uri.EscapeDataString(returnUrl)}";
    }

    using HttpClient client = new();
    using HttpResponseMessage response = await client.PostAsync(
        requestUri.Uri,
        null,
        cancellationToken);
    if (!response.IsSuccessStatusCode)
    {
        Console.Error.WriteLine(
            $"Espada rejected the bootstrap request ({(int)response.StatusCode}).");
        return null;
    }

    BootstrapLinkResponse? link =
        await response.Content.ReadFromJsonAsync<BootstrapLinkResponse>(
            cancellationToken);
    if (link is null || string.IsNullOrWhiteSpace(link.Url))
    {
        Console.Error.WriteLine(
            "Espada returned an invalid bootstrap response.");
        return null;
    }

    if (!Uri.TryCreate(
            link.Url,
            UriKind.RelativeOrAbsolute,
            out Uri? linkUri))
    {
        Console.Error.WriteLine(
            "Espada returned an invalid bootstrap URL.");
        return null;
    }

    Uri absoluteLink = linkUri.IsAbsoluteUri
        ? linkUri
        : new Uri(endpoint, linkUri);
    if (!IsLocalAuthority(absoluteLink))
    {
        Console.Error.WriteLine(
            "Espada returned a non-loopback bootstrap URL.");
        return null;
    }

    return link with { Url = absoluteLink.AbsoluteUri };
}

static bool IsLocalAuthority(Uri endpoint)
{
    if (!endpoint.IsAbsoluteUri
        || endpoint.Scheme != Uri.UriSchemeHttp)
    {
        return false;
    }

    return endpoint.Host.Equals(
               "localhost",
               StringComparison.OrdinalIgnoreCase)
           || (IPAddress.TryParse(
                   endpoint.Host,
                   out IPAddress? address)
               && IPAddress.IsLoopback(address));
}
