using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;

namespace Tracing.Client.Exporting;

internal static class StandardOtlpEndpoint
{
    internal const string ConfigurationKey = "OTEL_EXPORTER_OTLP_ENDPOINT";

    private const string UseOtlpExporterRegistrationName = "UseOtlpExporterRegistration";

    internal static void AddFallbackExporter(
        TracerProviderBuilder tracing,
        IConfiguration configuration,
        IServiceCollection services)
    {
        if (tracing is not IDeferredTracerProviderBuilder deferredTracing) return;

        deferredTracing.Configure((_, deferred) =>
        {
            var endpoint = ResolveFallback(configuration, services);
            if (endpoint is null) return;

            deferred.AddProcessor(new BatchActivityExportProcessor(
                new OtlpTraceExporter(new OtlpExporterOptions { Endpoint = endpoint })));
        });
    }

    internal static Uri? ResolveFallback(IConfiguration configuration, IServiceCollection services)
    {
        var endpoint = configuration[ConfigurationKey];
        var exportedByUseOtlpExporter = services.Any(
            descriptor => descriptor.ServiceType.Name == UseOtlpExporterRegistrationName);

        return string.IsNullOrWhiteSpace(endpoint) || exportedByUseOtlpExporter ? null : new Uri(endpoint);
    }
}
