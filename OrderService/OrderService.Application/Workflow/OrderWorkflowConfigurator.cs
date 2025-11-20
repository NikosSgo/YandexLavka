using OrderService.Application.Workflow.StageActions;
using OrderService.Domain.Enums;

namespace OrderService.Application.Workflow;

public static class OrderWorkflowConfigurator
{
    public static OrderStateMachine CreateDefault()
    {
        var actions = Enum.GetValues<OrderStatus>()
            .ToDictionary(status => status, status => (IOrderStageAction)new NoOpStageAction(status));

        var configurations = new List<OrderStageConfiguration>
        {
            new(OrderStatus.Initialized, new[] { OrderStatus.AwaitingPayment, OrderStatus.Cancelled }, actions[OrderStatus.Initialized]),
            new(OrderStatus.AwaitingPayment, new[] { OrderStatus.PaymentProcessing, OrderStatus.Cancelled }, actions[OrderStatus.AwaitingPayment]),
            new(OrderStatus.PaymentProcessing, new[] { OrderStatus.PaymentFailed, OrderStatus.PaymentConfirmed }, actions[OrderStatus.PaymentProcessing]),
            new(OrderStatus.PaymentFailed, new[] { OrderStatus.AwaitingPayment, OrderStatus.Cancelled }, actions[OrderStatus.PaymentFailed]),
            new(OrderStatus.PaymentConfirmed, new[] { OrderStatus.Picking, OrderStatus.Packing }, actions[OrderStatus.PaymentConfirmed]),
            new(OrderStatus.Picking, new[] { OrderStatus.Packing, OrderStatus.Cancelled }, actions[OrderStatus.Picking]),
            new(OrderStatus.Packing, new[] { OrderStatus.ReadyForDelivery }, actions[OrderStatus.Packing]),
            new(OrderStatus.ReadyForDelivery, new[] { OrderStatus.OutForDelivery }, actions[OrderStatus.ReadyForDelivery]),
            new(OrderStatus.OutForDelivery, new[] { OrderStatus.Delivered, OrderStatus.Cancelled }, actions[OrderStatus.OutForDelivery]),
            new(OrderStatus.Delivered, Array.Empty<OrderStatus>(), actions[OrderStatus.Delivered]),
            new(OrderStatus.Cancelled, Array.Empty<OrderStatus>(), actions[OrderStatus.Cancelled])
        };

        return new OrderStateMachine(configurations);
    }
}

