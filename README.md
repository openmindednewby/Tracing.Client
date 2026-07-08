# Tracing.Client

Shared distributed tracing configuration with OpenTelemetry for all SaaS services.

## Features

- Auto-instruments ASP.NET Core (inbound HTTP requests)
- Auto-instruments HttpClient (outbound HTTP calls)
- Auto-instruments Entity Framework Core (database queries)
- Auto-instruments MassTransit (RabbitMQ message consumers/publishers)
- Exports traces via OTLP to Jaeger or any OTLP-compatible backend
- Configurable sampling rate (100% in dev, 10% in production)
- W3C TraceContext propagation out of the box
- Graceful no-op when Jaeger is not running

## Quick Start

```csharp
// In Program.cs (builder phase)
builder.AddTracingDefaults(opts => opts.ServiceName = "IdentityService");
```

## Configuration

### Via appsettings.json

```json
{
  "Tracing": {
    "ServiceName": "IdentityService",
    "SamplingRatio": 0.1,
    "OtlpEndpoint": "http://jaeger:4317",
    "InstrumentEfCore": true,
    "InstrumentMassTransit": true,
    "IncludeDbStatements": false
  }
}
```

### Via Environment Variables

| Variable | Description |
|----------|-------------|
| `Tracing__OtlpEndpoint` | OTLP endpoint (maps to `Tracing:OtlpEndpoint` config key) |
| `Tracing__SamplingRatio` | Sampling ratio (0.0 to 1.0) |
| `Tracing__Enabled` | Enable/disable tracing entirely |

> **Note:** The standard `OTEL_EXPORTER_OTLP_ENDPOINT` env var is intentionally NOT read by this
> package to avoid conflicts with .NET Aspire's `UseOtlpExporter()`. Use `Tracing__OtlpEndpoint`.

### Via Docker Compose

```yaml
environment:
  - Tracing__OtlpEndpoint=http://jaeger:4317
  - Tracing__SamplingRatio=0.1
```

## Design Decisions

- **No-op when no endpoint**: If `Tracing:OtlpEndpoint` is empty (or the `Tracing__OtlpEndpoint`
  env var is unset), traces are collected but not exported. Services start without errors even
  if Jaeger is down.
- **MassTransit**: MassTransit 8.x has built-in OpenTelemetry via `System.Diagnostics.Activity`.
  We add its ActivitySource name so the OpenTelemetry SDK captures those spans automatically.
- **Sampling**: Defaults to 100% in development. Set to 10% in production via config or env var.
