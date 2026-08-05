namespace Espada.Protocol.Sync.Options
{
    public sealed class SyncServerOptions
    {
        public const string SectionName = "Sync:Server";

        public bool Enabled { get; init; }
        public int MaxDevices { get; init; }
        public long MaxStorageBytes { get; init; }
        public long MaxEgressBytes { get; init; }
        public int MaxPushEvents { get; init; }

        public bool IsValid()
        {
            return !Enabled || MaxDevices > 0 && MaxStorageBytes > 0 && MaxEgressBytes > 0 && MaxPushEvents > 0;
        }
    }
}