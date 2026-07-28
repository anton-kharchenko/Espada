using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppContainers;
using Azure.ResourceManager.AppContainers.Models;

namespace Espada.DeploymentKit.Azure
{
    internal static class ContainerAppJobRunner
    {
        public static async Task RunAsync(string subscriptionId, string resourceGroupName, string jobName,
            CancellationToken cancellationToken)
        {
            ArmClient client = new(new DefaultAzureCredential(), subscriptionId);
            ResourceIdentifier identifier =
                ContainerAppJobResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, jobName);
            ContainerAppJobResource job = client.GetContainerAppJobResource(identifier);

            ArmOperation<ContainerAppJobExecutionBase> operation = await job.StartAsync(WaitUntil.Completed,
                new ContainerAppJobExecutionTemplate(), cancellationToken);

            ContainerAppJobExecutionResource execution =
                client.GetContainerAppJobExecutionResource(new ResourceIdentifier(operation.Value.Id));
            DateTimeOffset deadline = DateTimeOffset.UtcNow.AddMinutes(15);

            while (DateTimeOffset.UtcNow < deadline)
            {
                Response<ContainerAppJobExecutionResource> response = await execution.GetAsync(cancellationToken);
                string? status = response.Value.Data.Status?.ToString();

                if (string.Equals(status, "Succeeded", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (string.Equals(status, "Failed", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(status, "Stopped", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Container Apps job '{jobName}' finished with status '{status}'.");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }

            throw new TimeoutException($"Container Apps job '{jobName}' did not finish within 15 minutes.");
        }
    }
}