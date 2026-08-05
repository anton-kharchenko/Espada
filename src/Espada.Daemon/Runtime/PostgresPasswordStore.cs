using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;

namespace Espada.Daemon.Runtime
{
    public sealed class PostgresPasswordStore
    {
        private readonly string _passwordFile;

        public PostgresPasswordStore(LocalRuntimePaths paths)
        {
            ArgumentNullException.ThrowIfNull(paths);
            _passwordFile = paths.PasswordFile;
        }

        public string GetOrCreate()
        {
            if (!File.Exists(_passwordFile))
            {
                string password = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
                File.WriteAllText(_passwordFile, password);
                ProtectFile(_passwordFile);
                return password;
            }

            ProtectFile(_passwordFile);
            string existing = File.ReadAllText(_passwordFile).Trim();
            return string.IsNullOrWhiteSpace(existing)
                ? throw new InvalidOperationException("PostgreSQL password file is empty.")
                : existing;
        }

        internal static void ProtectFile(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            if (OperatingSystem.IsWindows())
            {
                SecurityIdentifier currentUser = WindowsIdentity.GetCurrent().User
                    ?? throw new InvalidOperationException("Current Windows user SID is unavailable.");
                FileInfo file = new(path);
                FileSecurity security = file.GetAccessControl();
                security.SetAccessRuleProtection(true, false);
                security.SetAccessRule(new FileSystemAccessRule(
                    currentUser,
                    FileSystemRights.FullControl,
                    AccessControlType.Allow));
                file.SetAccessControl(security);
                return;
            }

            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }
}