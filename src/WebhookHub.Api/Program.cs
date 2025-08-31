using System.Diagnostics;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WebhookHub.Application;
using WebhookHub.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var serviceName = "webhook-hub-api";
builder.Services.AddSingleton(new ActivitySource(serviceName));

// CORS for SvelteKit dev
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins("http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r
        .AddService(serviceName)
        .AddEnvironmentVariableDetector())
    .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation();
        t.AddHttpClientInstrumentation();
        t.AddEntityFrameworkCoreInstrumentation();

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
app.MapControllers();

await app.RunAsync();
