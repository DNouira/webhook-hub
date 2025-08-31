using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebhookHub.Application.Health;

namespace WebhookHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ActivitySource _src;

    public HealthController(IMediator mediator, ActivitySource src)
    {
        _mediator = mediator;
        _src = src;
    }

    // Matches the existing Minimal API route. If you keep both, change one path to avoid conflicts.
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        using var act = _src.StartActivity("health-check(controller)");
        var dto = await _mediator.Send(new CreateHealthEventCommand("healthz"));
        return Ok(new { status = "ok", time = dto.ReceivedAt });
    }
}
