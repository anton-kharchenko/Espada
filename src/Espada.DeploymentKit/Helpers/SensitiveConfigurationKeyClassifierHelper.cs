namespace Espada.DeploymentKit.Helpers
{
    /// <summary>
    ///     Identifies configuration keys whose values must be stored as Pulumi secrets.
    /// </summary>
    public static class SensitiveConfigurationKeyClassifierHelper
    {
        private static readonly string[] SensitiveMarkers =
        [
            "password",
            "secret",
            "token",
            "apikey",
            "accesskey",
            "connectionstring",
            "clientsecret",
            "privatekey"
        ];

        public static bool IsSensitive(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            string normalizedKey = key
                .Replace(":", string.Empty, StringComparison.Ordinal)
                .Replace("_", string.Empty, StringComparison.Ordinal)
                .Replace(".", string.Empty, StringComparison.Ordinal)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .ToLowerInvariant();

            return SensitiveMarkers.Any(marker => normalizedKey.Contains(marker, StringComparison.Ordinal));
        }
    }
}