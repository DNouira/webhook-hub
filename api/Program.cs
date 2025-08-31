using System.Diagnostics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins("http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()
));

var serviceName = "webhook-hub-api";
builder.Services.AddSingleton(new ActivitySource(serviceName));
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r
        .AddService(serviceName)
        .AddEnvironmentVariableDetector()) // lets OTEL_RESOURCE_ATTRIBUTES/OTEL_SERVICE_NAME work if you use them later
    .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation();
        t.AddHttpClientInstrumentation();

        // Read standard OTEL_* vars (provided via launchSettings.json or real env)
        var endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
                       ?? "http://localhost:4317";
        var protocol = builder.Configuration["OTEL_EXPORTER_OTLP_PROTOCOL"]?.ToLowerInvariant();
        var headers  = builder.Configuration["OTEL_EXPORTER_OTLP_HEADERS"]; // e.g. "x-api-key=abc,foo=bar"

        t.AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri(endpoint);

            // default is gRPC; switch if you ever use HTTP/protobuf
            o.Protocol = protocol is "http" or "http/protobuf"
                ? OtlpExportProtocol.HttpProtobuf
                : OtlpExportProtocol.Grpc;

            if (!string.IsNullOrWhiteSpace(headers))
                o.Headers = headers;
        });
    });

var app = builder.Build();

app.UseCors();
app.MapGet("/healthz", (ActivitySource src) =>
{
    using var act = src.StartActivity("health-check");
    return Results.Ok(new { status = "ok", time = DateTimeOffset.UtcNow });
});

await app.RunAsync();
