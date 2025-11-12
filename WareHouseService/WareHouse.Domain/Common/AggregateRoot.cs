using WareHouse.Domain.Events;

namespace WareHouse.Domain.Common;

public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public virtual Guid Id { get; protected set; } // ✅ Должно быть virtual

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void UpdateTimestamps()
    {
        // Логика обновления временных меток
    }
}