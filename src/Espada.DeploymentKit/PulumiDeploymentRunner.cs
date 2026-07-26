using Espada.DeploymentKit.Azure;
using Espada.DeploymentKit.Constants;
using Espada.DeploymentKit.Enums;
using Espada.DeploymentKit.Helpers;
using Espada.DeploymentKit.Settings;
using Pulumi.Automation;
using System.Diagnostics;

namespace Espada.DeploymentKit;

public static class PulumiDeploymentRunner
{
    public static async Task ExecuteAsync(DeploymentSettings settings, DeploymentOperationType operationType, CancellationToken cancellationToken = default)
    {
        DeploymentSettingsValidatorHelper.Validate(settings);

        if (!Enum.IsDefined(operationType))
        {
            throw new ArgumentOutOfRangeException(nameof(operationType));
        }

        if (operationType == DeploymentOperationType.Preview)
        {
            WorkspaceStack previewStack = await CreateStackAsync(settings, cancellationToken);
            await previewStack.PreviewAsync(new PreviewOptions { OnStandardOutput = Console.WriteLine }, cancellationToken);
            return;
        }

        WorkspaceStack stack = await CreateStackAsync(settings, cancellationToken);
        await stack.PreviewAsync(new PreviewOptions { OnStandardOutput = Console.WriteLine }, cancellationToken);

        ResourceNames names = ResourceNames.Create(settings.EnvironmentType, settings.SubscriptionId);
        if (!await RegistryExistsAsync(settings, names, cancellationToken))
        {
            WorkspaceStack bootstrapStack = await CreateStackAsync(settings with { DeployWorkloads = false, ApiEnabled = false }, cancellationToken);
            await bootstrapStack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine }, cancellationToken);
        }

        await BuildImagesAsync(settings, names, cancellationToken);

        WorkspaceStack migrationStack = await CreateStackAsync(settings with { ApiEnabled = false }, cancellationToken);
        await migrationStack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine }, cancellationToken);

        await ContainerAppJobRunner.RunAsync(settings.SubscriptionId, names.ResourceGroup, names.MigrationJob, cancellationToken);

        WorkspaceStack applicationStack = await CreateStackAsync(settings with { ApiEnabled = true }, cancellationToken);
        await applicationStack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine }, cancellationToken);
    }

    private static InlineProgramArgs CreateProgram(DeploymentSettings settings)
    {
        Dictionary<string, string?> environmentVariables = new(StringComparer.Ordinal)
        {
            ["ARM_SUBSCRIPTION_ID"] = settings.SubscriptionId,
            ["ARM_TENANT_ID"] = settings.TenantId,
            ["ARM_USE_CLI"] = "true",
            [DeploymentConfigurationNames.AzureSubscriptionId] = settings.SubscriptionId,
            [DeploymentConfigurationNames.AzureTenantId] = settings.TenantId
        };

        return new InlineProgramArgs("espada", settings.StackName, PulumiFn.Create(() => EspadaAzureStack.Create(settings)))
        {
            WorkDir = settings.RepositoryRoot,
            EnvironmentVariables = environmentVariables
        };
    }

    private static async Task<WorkspaceStack> CreateStackAsync(DeploymentSettings settings, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(settings.BackendUrl))
        {
            Environment.SetEnvironmentVariable(DeploymentConfigurationNames.PulumiBackendUrl, settings.BackendUrl);
        }

        return await LocalWorkspace.CreateOrSelectStackAsync(CreateProgram(settings), cancellationToken);
    }

    private static async Task<bool> RegistryExistsAsync(DeploymentSettings settings, ResourceNames names, CancellationToken cancellationToken)
    {
        int exitCode = await RunProcessAsync(
            "az",
            [
                "acr", "show",
                "--name", names.Registry,
                "--resource-group", names.ResourceGroup,
                "--subscription", settings.SubscriptionId,
                "--only-show-errors"
            ],
            settings.RepositoryRoot,
            cancellationToken,
            ignoreFailure: true);

        return exitCode == 0;
    }

    private static async Task BuildImagesAsync(DeploymentSettings settings, ResourceNames names, CancellationToken cancellationToken)
    {
        await BuildImageAsync(settings, names, AzureDeploymentConstants.ApiImageRepository, AzureDeploymentConstants.ApiDockerfile, cancellationToken);
        await BuildImageAsync(settings, names, AzureDeploymentConstants.DatabaseImageRepository, AzureDeploymentConstants.DatabaseDockerfile, cancellationToken);
    }

    private static async Task BuildImageAsync(
        DeploymentSettings settings,
        ResourceNames names,
        string imageName,
        string dockerfile,
        CancellationToken cancellationToken) =>
        _ = await RunProcessAsync(
            "az",
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

    private static async Task<int> RunProcessAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string workingDirectory,
        CancellationToken cancellationToken,
        bool ignoreFailure = false)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = fileName,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false
        };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Could not start '{fileName}'.");
        await process.WaitForExitAsync(cancellationToken);

        if (!ignoreFailure && process.ExitCode != 0)
        {
            throw new InvalidOperationException($"'{fileName}' exited with code {process.ExitCode}.");
        }

        return process.ExitCode;
    }
}