using System.Text;

namespace Espada.Daemon.Runtime
{
    public static class RotatingLogFile
    {
        private const long MaximumBytes = 10 * 1024 * 1024;
        private const int MaximumFiles = 5;

        public static StreamWriter Open(string logDirectory, string name)
        {
            Directory.CreateDirectory(logDirectory);
            string path = Path.Join(logDirectory, $"{name}.log");
            if (File.Exists(path) && new FileInfo(path).Length >= MaximumBytes)
            {
                Rotate(path);
            }

            return new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read),
                new UTF8Encoding(false)) { AutoFlush = true };
        }

        private static void Rotate(string path)
        {
            string oldest = $"{path}.{MaximumFiles}";
            if (File.Exists(oldest))
            {
                File.Delete(oldest);
            }

            for (int index = MaximumFiles - 1; index >= 1; index--)
            {
                string source = $"{path}.{index}";
                if (File.Exists(source))
                {
                    File.Move(source, $"{path}.{index + 1}");
                }
            }

            File.Move(path, $"{path}.1");
        }
    }
}
