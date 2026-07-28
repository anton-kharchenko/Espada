namespace Espada.Application.Contracts.Jobs
{
    public interface IOutboxPublisher
    {
        Task<bool> PublishNextAsync(string leaseOwner, CancellationToken cancellationToken = default);
    }
}