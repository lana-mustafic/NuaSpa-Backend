namespace NuaSpa.Application.Messaging;

public sealed class NuaSpaMessageEnvelope
{
    public string Type { get; set; } = null!;
    public string PayloadJson { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
}
