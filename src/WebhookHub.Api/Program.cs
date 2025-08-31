using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WebhookHub.Application;
using WebhookHub.Application.Health;
using WebhookHub.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var serviceName = "webhook-hub-api";
builder.Services.AddSingleton(new ActivitySource(serviceName));

// CORS for SvelteKit dev
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins("http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r
        .AddService(serviceName)
        .AddEnvironmentVariableDetector())
    .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation();
        t.AddHttpClientInstrumentation();
        t.AddEntityFrameworkCoreInstrumentation(); // <-- EF spans

        var endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4317";
        var protocol = builder.Configuration["OTEL_EXPORTER_OTLP_PROTOCOL"]?.ToLowerInvariant();

        t.AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri(endpoint);
            o.Protocol = protocol is "http" or "http/protobuf"
                ? OtlpExportProtocol.HttpProtobuf
                : OtlpExportProtocol.Grpc;
        });
    });

// DI for layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("Db") ?? builder.Configuration["ConnectionStrings:Db"]);

var app = builder.Build();
app.UseCors();

app.MapGet("/healthz", async ([FromServices] ActivitySource src, [FromServices] MediatR.IMediator mediator) =>
{
    using var act = src.StartActivity("health-check");
    var dto = await mediator.Send(new CreateHealthEventCommand("healthz"));
    return Results.Ok(new { status = "ok", time = dto.ReceivedAt });
});

await app.RunAsync();
