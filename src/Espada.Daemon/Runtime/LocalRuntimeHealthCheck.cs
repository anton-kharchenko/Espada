using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Espada.Daemon.Runtime
{
    public sealed class LocalRuntimeHealthCheck(LocalRuntimeStatus status) : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(string.Equals(status.Status, "healthy", StringComparison.Ordinal)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"Local runtime is {status.Status}."));
        }
    }
}