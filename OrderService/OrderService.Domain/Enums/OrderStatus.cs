namespace OrderService.Domain.Enums;

public enum OrderStatus
{
    Initialized = 0,
    AwaitingPayment = 1,
    PaymentProcessing = 2,
    PaymentFailed = 3,
    PaymentConfirmed = 4,
    Picking = 5,
    Packing = 6,
    ReadyForDelivery = 7,
    OutForDelivery = 8,
    Delivered = 9,
    Cancelled = 10
}

