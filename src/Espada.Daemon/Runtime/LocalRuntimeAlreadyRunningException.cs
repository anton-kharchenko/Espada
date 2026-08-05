namespace Espada.Daemon.Runtime
{
    public sealed class LocalRuntimeAlreadyRunningException : InvalidOperationException
    {
        public LocalRuntimeAlreadyRunningException()
            : base("Espada daemon is already running for this user.")
        {
        }
    }
}