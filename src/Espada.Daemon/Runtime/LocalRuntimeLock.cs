using System.Globalization;

namespace Espada.Daemon.Runtime
{
    public sealed class LocalRuntimeLock : IDisposable
    {
        private readonly string _pidFile;
        private readonly FileStream _lockStream;
        private bool _disposed;

        private LocalRuntimeLock(string pidFile, FileStream lockStream)
        {
            _pidFile = pidFile;
            _lockStream = lockStream;
        }

        public static LocalRuntimeLock Acquire(LocalRuntimePaths paths)
        {
            ArgumentNullException.ThrowIfNull(paths);
            FileStream stream;
            try
            {
                stream = new FileStream(paths.LockFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                throw new LocalRuntimeAlreadyRunningException();
            }

            File.WriteAllText(paths.PidFile,
                Environment.ProcessId.ToString(CultureInfo.InvariantCulture));
            return new LocalRuntimeLock(paths.PidFile, stream);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _lockStream.Dispose();
            if (File.Exists(_pidFile))
            {
                File.Delete(_pidFile);
            }
        }
    }
}