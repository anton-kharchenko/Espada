using Espada.DeploymentKit.Azure;
using Espada.DeploymentKit.Constants;
using Espada.DeploymentKit.Enums;
using Espada.DeploymentKit.Helpers;
using Espada.DeploymentKit.Settings;
using Pulumi.Automation;
using System.Diagnostics;

namespace Espada.DeploymentKit
{
    public static class PulumiDeploymentRunner
    {
        private const string PulumiOrganization = "antonkharchenko";
        private const string PulumiProjectName = "espada";

        public static async Task ExecuteAsync(DeploymentSettings settings, DeploymentOperationType operationType,
            CancellationToken cancellationToken = default)
        {
            DeploymentSettingsValidatorHelper.Validate(settings);

            if (!Enum.IsDefined(operationType))
            {
                throw new ArgumentOutOfRangeException(nameof(operationType));
            }

            if (operationType == DeploymentOperationType.Preview)
            {
                WorkspaceStack previewStack = await CreateStackAsync(settings, cancellationToken);
                await previewStack.PreviewAsync(new PreviewOptions { OnStandardOutput = Console.WriteLine },
                    cancellationToken);
                return;
            }

            WorkspaceStack stack = await CreateStackAsync(settings, cancellationToken);
            await stack.PreviewAsync(new PreviewOptions { OnStandardOutput = Console.WriteLine }, cancellationToken);

            if (settings.TargetType == DeploymentTargetType.Website)
            {
                await stack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine }, cancellationToken);
                await DeployWebsiteAsync(settings, cancellationToken);
                return;
            }

            ResourceNames names = ResourceNames.Create(settings.EnvironmentType, settings.SubscriptionId);
            if (!await RegistryExistsAsync(settings, names, cancellationToken))
            {
                WorkspaceStack bootstrapStack =
                    await CreateStackAsync(settings with { DeployWorkloads = false, ApiEnabled = false },
                        cancellationToken);
                await bootstrapStack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine }, cancellationToken);
            }

            await BuildImagesAsync(settings, names, cancellationToken);

            WorkspaceStack migrationStack =
                await CreateStackAsync(settings with { ApiEnabled = false }, cancellationToken);
            await migrationStack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine }, cancellationToken);

            await ContainerAppJobRunner.RunAsync(settings.SubscriptionId, names.ResourceGroup, names.MigrationJob,
                cancellationToken);

            WorkspaceStack applicationStack =
                await CreateStackAsync(settings with { ApiEnabled = true }, cancellationToken);
            await applicationStack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine }, cancellationToken);

            if (settings.EnvironmentType == DeploymentEnvironmentType.Production)
            {
                await DeployWebsiteAsync(settings, cancellationToken);
            }
        }

        private static async Task DeployWebsiteAsync(DeploymentSettings settings, CancellationToken cancellationToken)
        {
            string websiteDirectory =
                Path.Join(settings.RepositoryRoot, AzureDeploymentConstants.WebsiteSourceDirectory);
            string npm = ResolveExecutable(OperatingSystem.IsWindows() ? "npm.cmd" : "npm");
            string npx = ResolveExecutable(OperatingSystem.IsWindows() ? "npx.cmd" : "npx");

            await RunProcessAsync(npm, ["ci"], websiteDirectory, cancellationToken);
            await RunProcessAsync(npm, ["run", "lint"], websiteDirectory, cancellationToken);
            await RunProcessAsync(npm, ["test"], websiteDirectory, cancellationToken);
            await RunProcessAsync(npm, ["run", "build"], websiteDirectory, cancellationToken);

            string deploymentToken = await GetWebsiteDeploymentTokenAsync(settings, cancellationToken);
            await RunProcessAsync(
                npx,
                [
                    "--yes",
                    AzureDeploymentConstants.StaticWebAppsCliPackage,
                    "deploy",
                    AzureDeploymentConstants.WebsiteDistDirectory,
                    "--env",
                    "production"
                ],
                websiteDirectory,
                cancellationToken,
                environmentVariables: new Dictionary<string, string>
                {
                    ["SWA_CLI_DEPLOYMENT_TOKEN"] = deploymentToken
                });
        }

        private static string ResolveAzureCli()
        {
            return ResolveExecutable(OperatingSystem.IsWindows() ? "az.cmd" : "az");
        }

        private static string ResolveExecutable(string fileName)
        {
            string? candidate = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(directory => Path.Join(directory.Trim('"'), fileName))
                .FirstOrDefault(File.Exists);

            return candidate ?? throw new FileNotFoundException($"Could not find '{fileName}' in PATH.");
        }

        private static async Task<string> GetWebsiteDeploymentTokenAsync(
            DeploymentSettings settings,
            CancellationToken cancellationToken)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = ResolveAzureCli(),
                WorkingDirectory = settings.RepositoryRoot,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            foreach (string argument in new[]
                     {
                         "staticwebapp", "secrets", "list", "--name",
                         AzureDeploymentConstants.WebsiteStaticSiteName, "--resource-group",
                         AzureDeploymentConstants.WebsiteResourceGroupName, "--subscription",
                         settings.SubscriptionId, "--query", "properties.apiKey", "--output", "tsv",
                         "--only-show-errors"
                     })
            {
                startInfo.ArgumentList.Add(argument);
            }

            using Process process = Process.Start(startInfo) ??
                                    throw new InvalidOperationException("Could not start Azure CLI.");
            Task<string> outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            Task<string> errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            string output = (await outputTask).Trim();
            string error = (await errorTask).Trim();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Azure CLI exited with code {process.ExitCode}: {error}");
            }

            return !string.IsNullOrWhiteSpace(output)
                ? output
                : throw new InvalidOperationException("Azure Static Web Apps returned an empty deployment token.");
        }

        private static InlineProgramArgs CreateProgram(DeploymentSettings settings)
        {
            Dictionary<string, string?> environmentVariables = new(StringComparer.Ordinal)
            {
                ["ARM_SUBSCRIPTION_ID"] = settings.SubscriptionId,
                ["ARM_TENANT_ID"] = settings.TenantId,
                ["ARM_USE_CLI"] = "true",
                [DeploymentConfigurationConstants.AzureSubscriptionId] = settings.SubscriptionId,
                [DeploymentConfigurationConstants.AzureTenantId] = settings.TenantId
            };

            return new InlineProgramArgs(
                PulumiProjectName,
                $"{PulumiOrganization}/{PulumiProjectName}/{settings.StackName}",
                PulumiFn.Create(() => EspadaAzureStack.Create(settings)))
            {
                WorkDir = settings.RepositoryRoot, EnvironmentVariables = environmentVariables
            };
        }

        private static async Task<WorkspaceStack> CreateStackAsync(DeploymentSettings settings,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(settings.BackendUrl))
            {
                Environment.SetEnvironmentVariable(DeploymentConfigurationConstants.PulumiBackendUrl, settings.BackendUrl);
            }

            return await LocalWorkspace.CreateOrSelectStackAsync(CreateProgram(settings), cancellationToken);
        }

        private static async Task<bool> RegistryExistsAsync(DeploymentSettings settings, ResourceNames names,
            CancellationToken cancellationToken)
        {
            int exitCode = await RunProcessAsync(
                ResolveAzureCli(),
                [
                    "acr", "show",
                    "--name", names.Registry,
                    "--resource-group", names.ResourceGroup,
                    "--subscription", settings.SubscriptionId,
                    "--only-show-errors"
                ],
                settings.RepositoryRoot,
                cancellationToken,
                true);

            return exitCode == 0;
        }

        private static async Task BuildImagesAsync(DeploymentSettings settings, ResourceNames names,
            CancellationToken cancellationToken)
        {
            await BuildImageAsync(settings, names, AzureDeploymentConstants.ApiImageRepository,
                AzureDeploymentConstants.ApiDockerfile, cancellationToken);
            await BuildImageAsync(
                settings,
                names,
                AzureDeploymentConstants.DatabaseImageRepository,
                AzureDeploymentConstants.DatabaseDockerfile,
                cancellationToken);
            await BuildImageAsync(
                settings,
                names,
                AzureDeploymentConstants.McpImageRepository,
                AzureDeploymentConstants.McpDockerfile,
                cancellationToken);
            await BuildImageAsync(settings, names, AzureDeploymentConstants.WorkerImageRepository,
                AzureDeploymentConstants.WorkerDockerfile, cancellationToken);
        }

        private static async Task BuildImageAsync(
            DeploymentSettings settings,
            ResourceNames names,
            string imageName,
            string dockerfile,
            CancellationToken cancellationToken)
        {
            _ = await RunProcessAsync(
                ResolveAzureCli(),
                [
                    "acr", "build",
                    "--registry", names.Registry,
                    "--image", $"{imageName}:{settings.ImageTag}",
                    "--file", dockerfile,
                    "--subscription", settings.SubscriptionId,
                    "--only-show-errors",
                    "."
                ],
                settings.RepositoryRoot,
                cancellationToken);
        }

        private static async Task<int> RunProcessAsync(
            string fileName,
            IReadOnlyList<string> arguments,
            string workingDirectory,
            CancellationToken cancellationToken,
            bool ignoreFailure = false,
            IReadOnlyDictionary<string, string>? environmentVariables = null)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = fileName, WorkingDirectory = workingDirectory, UseShellExecute = false
            };

            foreach (string argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            if (environmentVariables is not null)
            {
                foreach ((string name, string value) in environmentVariables)
                {
                    startInfo.Environment[name] = value;
                }
            }

            using Process process = Process.Start(startInfo) ??
                                    throw new InvalidOperationException($"Could not start '{fileName}'.");
            await process.WaitForExitAsync(cancellationToken);

            if (!ignoreFailure && process.ExitCode != 0)
            {
                throw new InvalidOperationException($"'{fileName}' exited with code {process.ExitCode}.");
            }

            return process.ExitCode;
        }
    }
}
