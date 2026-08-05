namespace Espada.Protocol.Sync.Contracts
{
    public sealed record RegisterDeviceRequest(Guid DeviceId, string Name);
}