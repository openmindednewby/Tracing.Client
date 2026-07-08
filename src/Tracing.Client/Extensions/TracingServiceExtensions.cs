using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tracing.Client.Configuration;

namespace Tracing.Client.Extensions;

/// <summary>
/// Extension methods for configuring distributed tracing with OpenTelemetry.
/// Provides auto-instrumentation for ASP.NET Core, HttpClient, Entity Framework Core,
/// and MassTransit. Exports traces via OTLP to Jaeger or any OTLP-compatible backend.
/// </summary>
public static class TracingServiceExtensions
{
    /// <summary>
    /// The MassTransit ActivitySource name. MassTransit 8.x publishes activities under this name.
    /// </summary>
    internal const string MassTransitActivitySource = "MassTransit";

    /// <summary>
    /// Adds distributed tracing to the application with OpenTelemetry.
    /// Auto-instruments ASP.NET Core, HttpClient, and optionally EF Core and MassTransit.
    /// Exports to OTLP when an endpoint is configured; otherwise traces are silently discarded.
    /// </summary>
    /// <typeparam name="TBuilder">The host builder type.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <param name="configure">Optional action to configure tracing options.</param>
    /// <returns>The builder for chaining.</returns>
    public static TBuilder AddTracingDefaults<TBuilder>(
        this TBuilder builder,
        Action<TracingOptions>? configure = null)
        where TBuilder : IHostApplicationBuilder
    {
        var options = BuildOptions(builder.Configuration, configure);

        if (!options.Enabled) return builder;

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
                resource.AddService(serviceName: options.ServiceName))
            .WithTracing(tracing =>
            {
                ConfigureSampler(tracing, options.SamplingRatio);
                AddInstrumentation(tracing, options);
                AddExporter(tracing, options, builder.Configuration);
            });

        return builder;
    }

    /// <summary>
    /// Builds the <see cref="TracingOptions"/> from configuration and programmatic overrides.
    /// </summary>
    /// <remarks>
    /// NOTE: The <c>OTEL_EXPORTER_OTLP_ENDPOINT</c> standard environment variable is intentionally
    /// NOT applied here. Services that need it (e.g. those using .NET Aspire ServiceDefaults)
    /// register <c>UseOtlpExporter()</c> separately. Applying it here would conflict with that
    /// registration and cause a <see cref="NotSupportedException"/> at startup.
    /// Set <c>Tracing:OtlpEndpoint</c> in appsettings or use a programmatic override instead.
    /// </remarks>
    internal static TracingOptions BuildOptions(
        IConfiguration configuration,
        Action<TracingOptions>? configure)
    {
        var options = new TracingOptions();

        // Bind from configuration first (appsettings.json "Tracing" section)
        configuration
            .GetSection(TracingOptions.SectionName)
            .Bind(options);

        // Apply programmatic overrides
        configure?.Invoke(options);

        return options;
    }

    /// <summary>
    /// Configures the trace sampler based on the sampling ratio.
    /// A ratio of 1.0 uses <see cref="AlwaysOnSampler"/>.
    /// A ratio of 0.0 uses <see cref="AlwaysOffSampler"/>.
    /// Any other value uses <see cref="TraceIdRatioBasedSampler"/>.
    /// </summary>
    internal static void ConfigureSampler(
        TracerProviderBuilder tracing,
        double samplingRatio)
    {
        var clampedRatio = Math.Clamp(samplingRatio, 0.0, 1.0);

        tracing.SetSampler(clampedRatio switch
        {
            >= 1.0 => new AlwaysOnSampler(),
            <= 0.0 => new AlwaysOffSampler(),
            _ => new TraceIdRatioBasedSampler(clampedRatio)
        });
    }

    /// <summary>
    /// Adds instrumentation sources (ASP.NET Core, HttpClient, EF Core, MassTransit).
    /// </summary>
    internal static void AddInstrumentation(
        TracerProviderBuilder tracing,
        TracingOptions options)
    {
        // Always instrument ASP.NET Core and HttpClient
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        // Optionally instrument Entity Framework Core
        if (options.InstrumentEfCore)
        {
            tracing.AddEntityFrameworkCoreInstrumentation(efOptions =>
            {
                efOptions.SetDbStatementForText = options.IncludeDbStatements;
            });
        }

        // MassTransit 8.x emits activities via System.Diagnostics.Activity.
        // We add its ActivitySource so OpenTelemetry captures those spans.
        if (options.InstrumentMassTransit)
        {
            tracing.AddSource(MassTransitActivitySource);
        }
    }

    /// <summary>
    /// Configures the OTLP exporter when an endpoint is available.
    /// When no endpoint is configured, traces are silently discarded (no-op).
    /// </summary>
    internal static void AddExporter(
        TracerProviderBuilder tracing,
        TracingOptions options,
        IConfiguration configuration)
    {
        var endpoint = options.OtlpEndpoint;

        if (string.IsNullOrWhiteSpace(endpoint)) return;

        tracing.AddOtlpExporter(otlp =>
        {
            otlp.Endpoint = new Uri(endpoint);
        });
    }
}
