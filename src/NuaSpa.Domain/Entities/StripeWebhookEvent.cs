namespace NuaSpa.Domain.Entities;

/// <summary>Idempotentna obrada Stripe webhook događaja (deduplikacija po Event ID).</summary>
public class StripeWebhookEvent
{
    public int Id { get; set; }
    public string StripeEventId { get; set; } = null!;
    public string EventType { get; set; } = null!;
    public DateTime ProcessedAtUtc { get; set; }
}
