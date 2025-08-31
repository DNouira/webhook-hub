namespace WebhookHub.Contracts;

public record EventDto(Guid Id, string Source, DateTimeOffset ReceivedAt);
