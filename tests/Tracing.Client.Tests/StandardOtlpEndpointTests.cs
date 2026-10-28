using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Tracing.Client.Exporting;
using Tracing.Client.Extensions;

namespace Tracing.Client.Tests;

public sealed class StandardOtlpEndpointTests
{
    private const string BoxTempo = "http://10.0.0.5:4317";

    [Fact]
    public void ResolveFallback_WithStandardVariableOnly_ReturnsEndpoint()
    {
        var configuration = ConfigurationWith(BoxTempo);
        var services = new ServiceCollection();

        var endpoint = StandardOtlpEndpoint.ResolveFallback(configuration, services);

        endpoint.Should().Be(new Uri(BoxTempo));
    }

    [Fact]
    public void ResolveFallback_WithoutStandardVariable_ReturnsNull()
    {
        var configuration = ConfigurationWith(null);
        var services = new ServiceCollection();

        var endpoint = StandardOtlpEndpoint.ResolveFallback(configuration, services);

        endpoint.Should().BeNull();
    }

    [Fact]
    public void ResolveFallback_WhenUseOtlpExporterRegistered_ReturnsNull()
    {
        var configuration = ConfigurationWith(BoxTempo);
        var services = new ServiceCollection();
        services.AddOpenTelemetry().UseOtlpExporter();

        var endpoint = StandardOtlpEndpoint.ResolveFallback(configuration, services);

        endpoint.Should().BeNull();
    }

    [Fact]
    public async Task AddTracingDefaults_WithStandardVariableAndUseOtlpExporter_BuildsTracerProvider()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration[StandardOtlpEndpoint.ConfigurationKey] = BoxTempo;
        builder.Services.AddOpenTelemetry().UseOtlpExporter();
        builder.AddTracingDefaults();
        await using var app = builder.Build();

        var provider = app.Services.GetService<TracerProvider>();

        provider.Should().NotBeNull();
    }

    [Fact]
    public async Task AddTracingDefaults_WithStandardVariableOnly_BuildsTracerProvider()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration[StandardOtlpEndpoint.ConfigurationKey] = BoxTempo;
        builder.AddTracingDefaults();
        await using var app = builder.Build();

        var provider = app.Services.GetService<TracerProvider>();

        provider.Should().NotBeNull();
    }

    private static IConfiguration ConfigurationWith(string? endpoint) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [StandardOtlpEndpoint.ConfigurationKey] = endpoint
            })
            .Build();
}
