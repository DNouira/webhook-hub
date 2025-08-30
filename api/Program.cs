using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

var serviceName = "webhook-hub-api";
builder.Services.AddSingleton(new ActivitySource(serviceName));

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName))
    .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation();
        t.AddHttpClientInstrumentation();
        t.AddOtlpExporter(o =>
        {
            var endpoint = Environment.GetEnvironmentVariable("OTLP_ENDPOINT")
                           ?? "http://localhost:4317";
            o.Endpoint = new Uri(endpoint);
        });
    });

var app = builder.Build();

app.MapGet("/healthz", (ActivitySource src) =>
{
    using var act = src.StartActivity("health-check");
    return Results.Ok(new { status = "ok", time = DateTimeOffset.UtcNow });
});

await app.RunAsync();