# Webhook Hub

A modular monolith for ingesting provider webhooks, storing/auditing events, and routing deliveries with observability built in.

- **Backend:** .NET (Minimal API) + Clean Architecture seams  
- **Frontend:** SvelteKit (TypeScript)  
- **Data:** Postgres (EF Core)  
- **Observability:** OpenTelemetry → OTLP → Jaeger  
- **Local Infra:** Docker Compose (Collector, Jaeger, Postgres)

---

## Repository layout

/src
WebhookHub.Api # HTTP edge (ASP.NET Core minimal API)
WebhookHub.Worker # Background processor (scaffolded)
WebhookHub.Application # Use cases (MediatR), validators, policies
WebhookHub.Domain # Pure entities/value objects (no EF/HTTP)
WebhookHub.Infrastructure # EF Core + Postgres; implementations of ports
WebhookHub.Contracts # DTOs shared across boundaries
/web # SvelteKit frontend (calls the API)
/infra # Docker Compose (Jaeger, OTel Collector, Postgres)
/tests # .NET test projects (placeholder)

yaml
Copy code

**Why this shape?** One deployable backend with strong boundaries for testability and future extraction if needed. The frontend stays separate (Node toolchain) so backend builds/containers remain lean.

---

## Architecture & project references

Domain ← Application ← Infrastructure
↑ ↑
└──── Api ──────┘
└──── Worker ───┘

Contracts is shared where DTOs are needed.

markdown
Copy code

- **Application** → references **Domain**, **Contracts**  
- **Infrastructure** → references **Application**, **Domain**, **Contracts**  
- **Api** → references **Application**, **Infrastructure**, **Contracts**  
- **Worker** → references **Application**, **Infrastructure**, **Contracts**

---

## Implemented vertical slice

**Domain**
- `Event` entity (Id, Source, ReceivedAt)

**Contracts**
- `EventDto` (edge DTO)

**Application**
- `IEventWriter` (port)
- `CreateHealthEventCommand` (MediatR) + handler
- `AddApplication()` DI extension (registers MediatR)

**Infrastructure**
- `AppDb` (EF Core `DbContext`, maps `Event` → `events`)
- `EventWriter` (implements `IEventWriter`)
- `AddInfrastructure()` DI extension (DbContext, `EventWriter`, connection)

**Api**
- CORS enabled for `http://localhost:5173`
- OpenTelemetry tracing (ASP.NET + HttpClient + EF Core) with **OTLP** exporter
- `GET /healthz` → starts an Activity, executes the use case, returns `{ status, time }`

**Effect:** calling `/healthz` inserts an `events` row and emits a distributed trace visible in **Jaeger**.

---

## Local infrastructure

- **OTel Collector** (OTLP receiver): `localhost:4317`  
- **Jaeger UI**: `http://localhost:16686`  
- **Postgres**: `localhost:5432` (default dev creds in compose)

Start:
```bash
cd infra
docker compose up -d
The API exports spans via OTLP → Collector → Jaeger.

Frontend (SvelteKit)
Location: /web

Env file: .env

ini
Copy code
PUBLIC_API_BASE_URL=http://localhost:8080
src/lib/api.ts performs fetch(BASE + path)

Home page (+page.ts / +page.svelte) calls /healthz, displays status/time, includes a Refresh button and a link to Jaeger.

Configuration
API launchSettings.json

Single profile at http://localhost:8080

Sets OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317

OpenTelemetry

Reads standard OTEL_* keys from configuration (launchSettings/env/appsettings), so no per-run shell variables are required.

CORS

Allows http://localhost:5173 for SvelteKit dev.

Packages of note
Api

OpenTelemetry.Extensions.Hosting – .AddOpenTelemetry() entrypoint

OpenTelemetry.Exporter.OpenTelemetryProtocol – OTLP exporter

OpenTelemetry.Instrumentation.AspNetCore – server spans

OpenTelemetry.Instrumentation.Http – HttpClient spans

OpenTelemetry.Instrumentation.EntityFrameworkCore (prerelease) – EF spans

Application

MediatR + MediatR.Extensions.Microsoft.DependencyInjection – command/handler pipeline

Infrastructure

Microsoft.EntityFrameworkCore, Npgsql.EntityFrameworkCore.PostgreSQL – EF Core + Postgres

Microsoft.EntityFrameworkCore.Design – migration tooling

Note: if package versions differ (e.g., MediatR core vs DI helper), align major versions to remove warnings.

Request flow
nginx
Copy code
Browser → SvelteKit → GET http://localhost:8080/healthz
       Api (ASP.NET) → Activity "health-check" starts
           ↓
       MediatR: CreateHealthEventCommand
           ↓
       Application handler calls IEventWriter
           ↓
       Infrastructure (EF Core) writes Event → Postgres
           ↓
       EF instrumentation emits spans; exporter sends via OTLP
           ↓
       Collector → Jaeger (inspect trace in UI)
Run locally
Infra

bash
Copy code
cd infra
docker compose up -d
Backend

bash
Copy code
dotnet run --project src/WebhookHub.Api
# API on http://localhost:8080
Frontend

bash
Copy code
cd web
npm run dev -- --port 5173
# Web on http://localhost:5173
Open:

Web UI: http://localhost:5173 (click Refresh)

Jaeger: http://localhost:16686 (service = webhook-hub-api)

Roadmap
/ingest/{provider} endpoints (GitHub, Stripe, etc.) with signature verification

Retry/delivery worker, DLQ, exponential backoff

Destination management, tenanting, authn/z

Search/index (e.g., Meilisearch) and object storage (MinIO)

CI/CD, containerized frontend, production adapters