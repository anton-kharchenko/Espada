using Espada.Application.Models;

namespace Espada.Worker.Repositories
{
    internal sealed class RepositoryWatchState : IDisposable
    {
        private readonly Lock _lock = new();
        private readonly FileSystemWatcher _watcher;
        private DateTimeOffset _lastSignalUtc;
        private bool _pending;

        public RepositoryWatchState(RepositoryWatchRegistration registration)
        {
            Registration = registration;
            _watcher = new FileSystemWatcher(registration.RepositoryRoot)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite |
                               NotifyFilters.Size,
                EnableRaisingEvents = true
            };
            _watcher.Changed += OnChanged;
            _watcher.Created += OnChanged;
            _watcher.Deleted += OnChanged;
            _watcher.Renamed += OnChanged;
            _watcher.Error += OnError;
            Signal();
        }

        public RepositoryWatchRegistration Registration { get; }

        public bool Matches(RepositoryWatchRegistration registration)
        {
            return Registration == registration;
        }

        public bool TryTake(DateTimeOffset nowUtc, TimeSpan debounce)
        {
            lock (_lock)
            {
                if (!_pending || nowUtc - _lastSignalUtc < debounce)
                {
                    return false;
                }

                _pending = false;
                return true;
            }
        }

        public void Signal()
        {
            lock (_lock)
            {
                _lastSignalUtc = DateTimeOffset.UtcNow;
                _pending = true;
            }
        }

        public void Dispose()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }

        private void OnChanged(object sender, FileSystemEventArgs eventArgs)
        {
            Signal();
        }

        private void OnError(object sender, ErrorEventArgs eventArgs)
        {
            Signal();
        }
    }
}