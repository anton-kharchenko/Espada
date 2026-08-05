namespace Espada.Infrastructure.Ingestion
{
    internal static class RepositoryPathResolver
    {
        public static string? Resolve(string root, string relativePath)
        {
            string rootPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
            string current = rootPath;
            foreach (string segment in relativePath.Replace('\\', '/')
                         .Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                if (segment == "..")
                {
                    return null;
                }

                current = Path.Join(current, segment);
                if (!File.Exists(current) && !Directory.Exists(current))
                {
                    return null;
                }

                FileAttributes attributes = File.GetAttributes(current);
                if ((attributes & FileAttributes.ReparsePoint) != 0)
                {
                    FileSystemInfo info = (attributes & FileAttributes.Directory) != 0
                        ? new DirectoryInfo(current)
                        : new FileInfo(current);
                    current = info.ResolveLinkTarget(true)?.FullName ?? string.Empty;
                    if (current.Length == 0)
                    {
                        return null;
                    }
                }

                if (!IsWithinRoot(rootPath, current))
                {
                    return null;
                }
            }

            return current;
        }

        private static bool IsWithinRoot(string root, string path)
        {
            string relative = Path.GetRelativePath(root, Path.GetFullPath(path));
            return !Path.IsPathRooted(relative) && relative != ".." &&
                   !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
        }
    }
}