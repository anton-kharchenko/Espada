namespace Espada.Infrastructure.Sync
{
    public sealed class SyncChangeSignal
    {
        private readonly SemaphoreSlim _signal = new(0, 1);

        public void Set()
        {
            if (_signal.CurrentCount == 0)
            {
                _signal.Release();
            }
        }

        public Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            return _signal.WaitAsync(timeout, cancellationToken);
        }
    }
}