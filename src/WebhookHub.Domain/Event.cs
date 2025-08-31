namespace WebhookHub.Domain;

public class Event
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Source { get; init; } = default!;
    public DateTimeOffset ReceivedAt { get; init; } = DateTimeOffset.UtcNow;
}
