using Espada.Deployment.Constants;
using Espada.DeploymentKit;
using Espada.DeploymentKit.Constants;
using Espada.DeploymentKit.Enums;
using Espada.DeploymentKit.Helpers;
using Espada.DeploymentKit.Settings;

namespace Espada.Deployment
{
    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                DeploymentOperationType operationType = args.FirstOrDefault()?.ToLowerInvariant() switch
                {
                    DeploymentCommandLineConstants.PreviewCommand => DeploymentOperationType.Preview,
                    DeploymentCommandLineConstants.DeployCommand => DeploymentOperationType.Deploy,
                    _ => throw new ArgumentException(
                        $"Command must be '{DeploymentCommandLineConstants.PreviewCommand}' or '{DeploymentCommandLineConstants.DeployCommand}'.")
                };

                Dictionary<string, string> options = ParseOptions(args.Skip(1));
                string repositoryRoot = options.GetValueOrDefault(DeploymentCommandLineConstants.RepositoryRootOption)
                                        ?? FindRepositoryRoot();
                DeploymentTargetType targetType = DeploymentTargetHelper.Parse(
                    RequireOption(options, DeploymentCommandLineConstants.TargetOption));
                DeploymentSettings settings = new(
                    DeploymentEnvironmentHelper.Parse(RequireOption(options,
                        DeploymentCommandLineConstants.EnvironmentOption)),
                    targetType,
                    RequireOption(options, DeploymentCommandLineConstants.LocationOption),
                    RequireEnvironmentVariable(DeploymentConfigurationConstants.AzureSubscriptionId),
                    RequireEnvironmentVariable(DeploymentConfigurationConstants.AzureTenantId),
                    targetType == DeploymentTargetType.All
                        ? RequireEnvironmentVariable(DeploymentConfigurationConstants.ApiKey)
                        : null,
                    options.GetValueOrDefault(DeploymentCommandLineConstants.ImageTagOption)
                    ?? Environment.GetEnvironmentVariable(DeploymentConfigurationConstants.GitCommitSha)
                    ?? DeploymentCommandLineConstants.LocalImageTag,
                    repositoryRoot,
                    Environment.GetEnvironmentVariable(DeploymentConfigurationConstants.PulumiBackendUrl))
                {
                    McpEntraAuthority = targetType == DeploymentTargetType.All
                        ? RequireEnvironmentVariable(
                            DeploymentConfigurationConstants.McpEntraAuthority)
                        : null,
                    McpEntraClientId = targetType == DeploymentTargetType.All
                        ? RequireEnvironmentVariable(
                            DeploymentConfigurationConstants.McpEntraClientId)
                        : null,
                    McpEntraClientSecret = targetType == DeploymentTargetType.All
                        ? RequireEnvironmentVariable(
                            DeploymentConfigurationConstants.McpEntraClientSecret)
                        : null,
                    McpSigningCertificate = targetType == DeploymentTargetType.All
                        ? RequireEnvironmentVariable(
                            DeploymentConfigurationConstants.McpSigningCertificate)
                        : null,
                    McpEncryptionCertificate = targetType == DeploymentTargetType.All
                        ? RequireEnvironmentVariable(
                            DeploymentConfigurationConstants.McpEncryptionCertificate)
                        : null
                };

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

        private static string RequireOption(IReadOnlyDictionary<string, string> options, string name)
        {
            return options.TryGetValue(name, out string? value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : throw new ArgumentException($"Option '--{name}' is required.");
        }

        private static string RequireEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
                ? value
                : throw new InvalidOperationException($"Environment variable '{name}' is required.");
        }

        private static string FindRepositoryRoot()
        {
            DirectoryInfo? directory = new(AppContext.BaseDirectory);
            while (directory is not null)
            {
                if (File.Exists(Path.Join(directory.FullName, DeploymentCommandLineConstants.SolutionFile)))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException(
                $"Could not find {DeploymentCommandLineConstants.SolutionFile}. Pass --{DeploymentCommandLineConstants.RepositoryRootOption}.");
        }
    }
}