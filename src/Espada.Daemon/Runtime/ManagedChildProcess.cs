using System.Diagnostics;

namespace Espada.Daemon.Runtime
{
    public sealed class ManagedChildProcess : IAsyncDisposable
    {
        private readonly Process _process;
        private readonly Task _standardOutput;
        private readonly Task _standardError;
        private readonly StreamWriter _log;

        public ManagedChildProcess(Process process, Task standardOutput, Task standardError, StreamWriter log)
        {
            _process = process;
            _standardOutput = standardOutput;
            _standardError = standardError;
            _log = log;
        }

        public int Id => _process.Id;

        public bool HasExited => _process.HasExited;
        public Task WaitForExitAsync(CancellationToken cancellationToken = default)
        {
            return _process.WaitForExitAsync(cancellationToken);
        }

        public async Task StopAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (_process.HasExited)
            {
                return;
            }

            using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);
            timeoutSource.CancelAfter(timeout);
            try
            {
                await _process.WaitForExitAsync(timeoutSource.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _process.Kill(true);
                await _process.WaitForExitAsync(CancellationToken.None);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!_process.HasExited)
            {
                _process.Kill(true);
            }

            await Task.WhenAll(_standardOutput, _standardError);
            await _log.DisposeAsync();
            _process.Dispose();
        }
    }
}
