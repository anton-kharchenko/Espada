namespace Espada.Daemon.Runtime
{
    public sealed class LocalRuntimeStatus
    {
        private readonly object _sync = new();
        private string _status = "starting";

        public string Status
        {
            get
            {
                lock (_sync)
                {
                    return _status;
                }
            }
        }

        public void Set(string status)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(status);
            lock (_sync)
            {
                _status = status;
            }
        }
    }
}