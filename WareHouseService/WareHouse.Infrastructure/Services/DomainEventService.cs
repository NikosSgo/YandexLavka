using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Domain.Events;

namespace WareHouse.Infrastructure.Services;

public interface IDomainEventService
{
    Task PublishDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents);
}

public class DomainEventService : IDomainEventService
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventService> _logger;

    public DomainEventService(IMediator mediator, ILogger<DomainEventService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task PublishDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            _logger.LogInformation("Publishing domain event: {EventType} {EventId}",
                domainEvent.GetType().Name, domainEvent.EventId);

            await _mediator.Publish(domainEvent);
        }
    }
}