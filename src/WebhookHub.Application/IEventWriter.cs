using WebhookHub.Domain;

namespace WebhookHub.Application.Abstractions;

public interface IEventWriter
{
    Task<Event> AddAsync(Event e, CancellationToken ct);
}
