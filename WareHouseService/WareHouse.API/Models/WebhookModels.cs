namespace WareHouse.API.Models;

public class WebhookSubscription
{
    public Guid Id { get; set; }
    public string CallbackUrl { get; set; }
    public string EventType { get; set; } // "order.picking_started", "order.picking_completed", etc.
    public string Secret { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WebhookPayload
{
    public string EventType { get; set; }
    public object Data { get; set; }
    public DateTime Timestamp { get; set; }
    public string Signature { get; set; }
}

public class CreateWebhookSubscriptionRequest
{
    public string CallbackUrl { get; set; }
    public string EventType { get; set; }
}

public class WebhookNotification
{
    public Guid OrderId { get; set; }
    public string EventType { get; set; }
    public object Payload { get; set; }
    public DateTime OccurredAt { get; set; }
}