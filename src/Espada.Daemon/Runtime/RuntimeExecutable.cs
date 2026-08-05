namespace Espada.Daemon.Runtime
{
    public sealed record RuntimeExecutable(string FileName, IReadOnlyList<string> PrefixArguments)
    {
        public static RuntimeExecutable Resolve(string applicationName, string? configuredPath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(applicationName);
            string? path = ResolvePath(applicationName, configuredPath);
            if (path is null)
            {
                throw new FileNotFoundException(
                    $"Runtime executable for {applicationName} was not found. Configure its absolute path.");
            }

            return string.Equals(Path.GetExtension(path), ".dll", StringComparison.OrdinalIgnoreCase)
                ? new RuntimeExecutable("dotnet", [path])
                : new RuntimeExecutable(path, []);
        }

        public ProcessCommand CreateCommand(IEnumerable<string> arguments,
            IReadOnlyDictionary<string, string?>? environment = null)
        {
            return new ProcessCommand(FileName, [.. PrefixArguments, .. arguments],
                Path.GetDirectoryName(PrefixArguments.FirstOrDefault() ?? FileName), environment);
        }

        private static string? ResolvePath(string applicationName, string? configuredPath)
        {
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                string fullPath = Path.GetFullPath(configuredPath);
                return File.Exists(fullPath) ? fullPath : null;
            }

            string executableName = OperatingSystem.IsWindows() ? $"{applicationName}.exe" : applicationName;
            string executablePath = Path.Join(AppContext.BaseDirectory, executableName);
            if (File.Exists(executablePath))
            {
                return executablePath;
            }

            string assemblyPath = Path.Join(AppContext.BaseDirectory, $"{applicationName}.dll");
            return File.Exists(assemblyPath) ? assemblyPath : null;
        }
    }
}