# Webhook Hub (MVP)
.NET 8 Minimal API + OpenTelemetry → Jaeger.

1) `cd infra && docker compose up -d`
2) In another terminal: `set OTLP_ENDPOINT=http://localhost:4317 && dotnet run --project api --urls http://localhost:8080`
3) Hit `http://localhost:8080/healthz` and check Jaeger at `http://localhost:16686` (service: `webhook-hub-api`).

- npm run dev           # start Vite dev server (HMR) on 5173
- npm run build         # production build (adapter decides output)
- npm test              # run Vitest unit/component tests
- npm run lint          # ESLint
- npm run format        # Prettier

api/
dotnet run

That will use your launchSettings.json (single Api profile), so it should:

listen on http://localhost:8080

set OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317

If you’re at the repo root
dotnet run --project api

If it doesn’t come up on 8080

Specify the profile explicitly:

dotnet run --project api --launch-profile Api
