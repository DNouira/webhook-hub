using WebhookHub.Application.Abstractions;
using WebhookHub.Domain;

namespace WebhookHub.Infrastructure.Persistence;

public class EventWriter(AppDb db) : IEventWriter
{
    public async Task<Event> AddAsync(Event e, CancellationToken ct)
    {
        db.Events.Add(e);
        await db.SaveChangesAsync(ct);
        return e;
    }
}
