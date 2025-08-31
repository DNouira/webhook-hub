using MediatR;
using WebhookHub.Contracts;
using WebhookHub.Domain;
using WebhookHub.Application.Abstractions;

namespace WebhookHub.Application.Health;

public record CreateHealthEventCommand(string Source) : IRequest<EventDto>;

public class CreateHealthEventHandler(IEventWriter writer) : IRequestHandler<CreateHealthEventCommand, EventDto>
{
    public async Task<EventDto> Handle(CreateHealthEventCommand request, CancellationToken ct)
    {
        var created = await writer.AddAsync(new Event { Source = request.Source }, ct);
        return new EventDto(created.Id, created.Source, created.ReceivedAt);
    }
}
