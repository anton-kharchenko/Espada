namespace Espada.Domain.SeedWork
{
    public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents where TId : notnull
    {
        private readonly List<IDomainEvent> _domainEvents = [];

        protected AggregateRoot()
        {
        }

        protected AggregateRoot(TId id) : base(id)
        {
        }

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public IReadOnlyCollection<IDomainEvent> DequeueDomainEvents()
        {
            if (_domainEvents.Count == 0)
            {
                return [];
            }

            IDomainEvent[] events = _domainEvents.ToArray();

            _domainEvents.Clear();

            return events;
        }

        protected void RaiseDomainEvent(IDomainEvent domainEvent)
        {
            ArgumentNullException.ThrowIfNull(domainEvent);

            _domainEvents.Add(domainEvent);
        }
    }
}