using Espada.DeploymentKit;
using Espada.DeploymentKit.Constants;
using Espada.DeploymentKit.Enums;
using Espada.DeploymentKit.Helpers;
using Espada.DeploymentKit.Settings;

namespace Espada.Deployment;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            DeploymentOperationType operationType = args.FirstOrDefault()?.ToLowerInvariant() switch
            {
                DeploymentCommandLineNames.PreviewCommand => DeploymentOperationType.Preview,
                DeploymentCommandLineNames.DeployCommand => DeploymentOperationType.Deploy,
                _ => throw new ArgumentException(
                    $"Command must be '{DeploymentCommandLineNames.PreviewCommand}' or '{DeploymentCommandLineNames.DeployCommand}'.")
            };

            Dictionary<string, string> options = ParseOptions(args.Skip(1));
            string repositoryRoot = options.GetValueOrDefault(DeploymentCommandLineNames.RepositoryRootOption)
                ?? FindRepositoryRoot();
            DeploymentTargetType targetType = DeploymentTargetHelper.Parse(
                RequireOption(options, DeploymentCommandLineNames.TargetOption));
            DeploymentSettings settings = new(
                DeploymentEnvironmentHelper.Parse(RequireOption(options, DeploymentCommandLineNames.EnvironmentOption)),
                targetType,
                RequireOption(options, DeploymentCommandLineNames.LocationOption),
                RequireEnvironmentVariable(DeploymentConfigurationNames.AzureSubscriptionId),
                RequireEnvironmentVariable(DeploymentConfigurationNames.AzureTenantId),
                targetType == DeploymentTargetType.All
                    ? RequireEnvironmentVariable(DeploymentConfigurationNames.ApiKey)
                    : null,
                options.GetValueOrDefault(DeploymentCommandLineNames.ImageTagOption)
                ?? Environment.GetEnvironmentVariable(DeploymentConfigurationNames.GitCommitSha)
                ?? DeploymentCommandLineNames.LocalImageTag,
                repositoryRoot,
                Environment.GetEnvironmentVariable(DeploymentConfigurationNames.PulumiBackendUrl));

            using CancellationTokenSource cancellation = new();
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cancellation.Cancel();
            };

            await PulumiDeploymentRunner.ExecuteAsync(
                settings,
                operationType,
                cancellation.Token);
            return 0;
        }
        catch (OperationCanceledException)
        {
            await Console.Error.WriteLineAsync("Deployment cancelled.");
            return 130;
        }
    }

    private static Dictionary<string, string> ParseOptions(IEnumerable<string> arguments)
    {
        string[] values = arguments.ToArray();
        Dictionary<string, string> options = new(StringComparer.OrdinalIgnoreCase);

        for (int index = 0; index < values.Length; index += 2)
        {
            string name = values[index];
            if (!name.StartsWith("--", StringComparison.Ordinal)
                || index + 1 >= values.Length
                || values[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Invalid option near '{name}'.");
            }

            options[name[2..]] = values[index + 1];
        }

        return options;
    }

    private static string RequireOption(IReadOnlyDictionary<string, string> options, string name) =>
        options.TryGetValue(name, out string? value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException($"Option '--{name}' is required.");

    private static string RequireEnvironmentVariable(string name) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"Environment variable '{name}' is required.");

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Join(directory.FullName, DeploymentCommandLineNames.SolutionFile)))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not find {DeploymentCommandLineNames.SolutionFile}. Pass --{DeploymentCommandLineNames.RepositoryRootOption}.");
    }
}