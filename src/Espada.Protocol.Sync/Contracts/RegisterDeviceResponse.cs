namespace Espada.Protocol.Sync.Contracts
{
    public sealed record RegisterDeviceResponse(Guid DeviceId, DateTimeOffset RegisteredAtUtc);
}