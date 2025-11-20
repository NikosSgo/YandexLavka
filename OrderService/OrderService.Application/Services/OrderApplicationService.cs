using Microsoft.Extensions.Logging;
using OrderService.Application.Abstractions;
using OrderService.Application.Contracts;
using OrderService.Application.Mappers;
using OrderService.Domain.Entities;

namespace OrderService.Application.Services;

public class OrderApplicationService : IOrderApplicationService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderStateMachine _stateMachine;
    private readonly ILogger<OrderApplicationService> _logger;

    public OrderApplicationService(
        IOrderRepository orderRepository,
        IOrderStateMachine stateMachine,
        ILogger<OrderApplicationService> logger)
    {
        _orderRepository = orderRepository;
        _stateMachine = stateMachine;
        _logger = logger;
    }

    public async Task<OrderDetailsDto> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var address = OrderMapper.ToDomain(command.DeliveryAddress);
        var items = OrderMapper.ToDomain(command.Items).ToArray();

        var order = new Order(
            Guid.NewGuid(),
            command.UserId,
            address,
            items,
            command.Metadata);

        await _orderRepository.AddAsync(order, cancellationToken);
        _logger.LogInformation("Order {OrderId} created for user {UserId}", order.Id, order.UserId);

        return OrderMapper.ToDetailsDto(order);
    }

    public async Task<OrderDetailsDto?> GetAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        return order is null ? null : OrderMapper.ToDetailsDto(order);
    }

    public async Task<OrderDetailsDto> AdvanceStatusAsync(
        AdvanceOrderStatusCommand command,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);

        if (order is null)
        {
            throw new KeyNotFoundException($"Order {command.OrderId} was not found");
        }

        await _stateMachine.TransitionAsync(order, command.TargetStatus, command.Actor, command.Notes, cancellationToken);
        await _orderRepository.UpdateAsync(order, cancellationToken);

        _logger.LogInformation(
            "Order {OrderId} moved to {Status} by {Actor}",
            order.Id,
            order.Status,
            command.Actor ?? "system");

        return OrderMapper.ToDetailsDto(order);
    }
}

