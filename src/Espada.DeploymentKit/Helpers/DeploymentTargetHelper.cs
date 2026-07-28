using Espada.DeploymentKit.Enums;

namespace Espada.DeploymentKit.Helpers
{
    public static class DeploymentTargetHelper
    {
        public static DeploymentTargetType Parse(string value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            return value.Trim().ToLowerInvariant() switch
            {
                "website" => DeploymentTargetType.Website,
                "all" => DeploymentTargetType.All,
                _ => throw new ArgumentException("Target must be website or all.", nameof(value))
            };
        }
    }
}