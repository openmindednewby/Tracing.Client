using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tracing.Client.Configuration;
using Tracing.Client.Exporting;
using Tracing.Client.Responses;

namespace Tracing.Client.Extensions;

/// <summary>Extension methods for configuring distributed tracing with OpenTelemetry.</summary>
public static class TracingServiceExtensions
{
    internal const string MassTransitActivitySource = "MassTransit";

    /// <summary>Adds distributed tracing to the application with OpenTelemetry.</summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="configure">Optional action to configure tracing options.</param>
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
                AddExporter(tracing, options, builder.Configuration, builder.Services);
            });

        TraceIdResponses.Register(builder.Services);

        return builder;
    }

    internal static TracingOptions BuildOptions(
        IConfiguration configuration,
        Action<TracingOptions>? configure)
    {
        var options = new TracingOptions();

        configuration
            .GetSection(TracingOptions.SectionName)
            .Bind(options);

        configure?.Invoke(options);

        return options;
    }

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

    internal static void AddInstrumentation(
        TracerProviderBuilder tracing,
        TracingOptions options)
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        if (options.InstrumentEfCore)
        {
            tracing.AddEntityFrameworkCoreInstrumentation(efOptions =>
            {
                efOptions.SetDbStatementForText = options.IncludeDbStatements;
            });
        }

        if (options.InstrumentMassTransit)
        {
            tracing.AddSource(MassTransitActivitySource);
        }
    }

    internal static void AddExporter(
        TracerProviderBuilder tracing,
        TracingOptions options,
        IConfiguration configuration,
        IServiceCollection services)
    {
        var endpoint = options.OtlpEndpoint;

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            StandardOtlpEndpoint.AddFallbackExporter(tracing, configuration, services);
            return;
        }

        tracing.AddOtlpExporter(otlp =>
        {
            otlp.Endpoint = new Uri(endpoint);
        });
    }
}
