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

    /// <summary>The service name used as the OpenTelemetry resource attribute.</summary>
    public string ServiceName { get; set; } = "Unknown";

    /// <summary>Whether tracing is enabled.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The OTLP endpoint; when empty, <c>OTEL_EXPORTER_OTLP_ENDPOINT</c> is used unless <c>UseOtlpExporter()</c> exports it.
    /// </summary>
    public string OtlpEndpoint { get; set; } = string.Empty;

    /// <summary>The sampling ratio for traces (0.0 to 1.0).</summary>
    public double SamplingRatio { get; set; } = 1.0;

    /// <summary>Whether to instrument Entity Framework Core database queries.</summary>
    public bool InstrumentEfCore { get; set; } = true;

    /// <summary>Whether to instrument MassTransit message consumers and publishers.</summary>
    public bool InstrumentMassTransit { get; set; } = true;

    /// <summary>Whether to set the <c>db.statement</c> attribute on EF Core spans.</summary>
    public bool IncludeDbStatements { get; set; } = true;
}
