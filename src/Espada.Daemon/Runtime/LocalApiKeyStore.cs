using System.Security.Cryptography;

namespace Espada.Daemon.Runtime
{
    public sealed class LocalApiKeyStore(LocalRuntimePaths paths)
    {
        public string GetOrCreate()
        {
            string path = paths.ApiKeyFile;
            if (File.Exists(path))
            {
                return File.ReadAllText(path).Trim();
            }

            string apiKey = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));
            File.WriteAllText(path, apiKey);
            PostgresPasswordStore.ProtectFile(path);
            return apiKey;
        }
    }
}