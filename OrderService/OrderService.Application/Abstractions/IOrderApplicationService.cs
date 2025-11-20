using OrderService.Application.Contracts;

namespace OrderService.Application.Abstractions;

public interface IOrderApplicationService
{
    Task<OrderDetailsDto> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken);

    Task<OrderDetailsDto?> GetAsync(Guid orderId, CancellationToken cancellationToken);

    Task<OrderDetailsDto> AdvanceStatusAsync(
        AdvanceOrderStatusCommand command,
        CancellationToken cancellationToken);
}

