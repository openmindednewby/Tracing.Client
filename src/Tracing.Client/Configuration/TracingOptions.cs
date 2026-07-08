namespace Tracing.Client.Configuration;

/// <summary>
/// Configuration options for distributed tracing with OpenTelemetry.
/// </summary>
public sealed class TracingOptions
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Tracing";

    /// <summary>
    /// The service name used as the OpenTelemetry resource attribute.
    /// This appears as the service name in Jaeger UI.
    /// </summary>
    public string ServiceName { get; set; } = "Unknown";

    /// <summary>
    /// Whether tracing is enabled. When false, no traces are collected or exported.
    /// Default: true (traces are collected but only exported when an OTLP endpoint is configured).
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The OTLP exporter endpoint (e.g., "http://jaeger:4317").
    /// When empty, traces are collected but not exported (no-op exporter).
    /// Set via <c>Tracing:OtlpEndpoint</c> in appsettings or <c>Tracing__OtlpEndpoint</c> env var.
    /// Note: <c>OTEL_EXPORTER_OTLP_ENDPOINT</c> is intentionally NOT read to avoid conflicts
    /// with .NET Aspire's <c>UseOtlpExporter()</c>.
    /// </summary>
    public string OtlpEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// The sampling ratio for traces (0.0 to 1.0).
    /// A value of 1.0 captures every trace; 0.1 captures 10%.
    /// Default: 1.0 (capture all in development).
    /// Override via appsettings or <c>Tracing:SamplingRatio</c> config key.
    /// </summary>
    public double SamplingRatio { get; set; } = 1.0;

    /// <summary>
    /// Whether to instrument Entity Framework Core database queries.
    /// Default: true.
    /// </summary>
    public bool InstrumentEfCore { get; set; } = true;

    /// <summary>
    /// Whether to instrument MassTransit message consumers and publishers.
    /// MassTransit 8.x has built-in OpenTelemetry support via System.Diagnostics.Activity.
    /// Default: true.
    /// </summary>
    public bool InstrumentMassTransit { get; set; } = true;

    /// <summary>
    /// Whether to set the <c>db.statement</c> attribute on EF Core spans.
    /// Includes the SQL query text. Disable in production if queries contain sensitive data.
    /// Default: true (useful for debugging in development).
    /// </summary>
    public bool IncludeDbStatements { get; set; } = true;
}
