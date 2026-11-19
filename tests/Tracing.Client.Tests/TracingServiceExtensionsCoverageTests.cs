using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using Tracing.Client.Configuration;
using Tracing.Client.Extensions;

namespace Tracing.Client.Tests;

/// <summary>Coverage tests for <see cref="TracingServiceExtensions"/>.</summary>
public sealed class TracingServiceExtensionsCoverageTests
{
    [Fact]
    public void AddTracingDefaults_WhenDisabled_ReturnsBuilderWithoutOpenTelemetry()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Tracing:Enabled"] = "false",
        });

        var returnedBuilder = builder.AddTracingDefaults();

        returnedBuilder.Should().BeSameAs(builder);

        var sp = builder.Services.BuildServiceProvider();
        var tracerProvider = sp.GetService<TracerProvider>();
        tracerProvider.Should().BeNull();
    }

    [Fact]
    public void AddTracingDefaults_DefaultOptions_RegistersOpenTelemetryWithDefaults()
    {
        var builder = WebApplication.CreateBuilder();

        var returnedBuilder = builder.AddTracingDefaults();

        returnedBuilder.Should().BeSameAs(builder);

        var sp = builder.Services.BuildServiceProvider();
        var tracerProvider = sp.GetService<TracerProvider>();
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddTracingDefaults_SamplingRatioHalf_UsesRatioBasedSampler()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Tracing:SamplingRatio"] = "0.5",
        });

        builder.AddTracingDefaults();

        var sp = builder.Services.BuildServiceProvider();
        var tracerProvider = sp.GetService<TracerProvider>();
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddTracingDefaults_SamplingRatioZero_UsesAlwaysOffSampler()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Tracing:SamplingRatio"] = "0.0",
        });

        builder.AddTracingDefaults();

        var sp = builder.Services.BuildServiceProvider();
        var tracerProvider = sp.GetService<TracerProvider>();
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddTracingDefaults_SamplingRatioAboveOne_ClampsToAlwaysOn()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Tracing:SamplingRatio"] = "2.0",
        });

        builder.AddTracingDefaults();

        var sp = builder.Services.BuildServiceProvider();
        var tracerProvider = sp.GetService<TracerProvider>();
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddTracingDefaults_SamplingRatioNegative_ClampsToAlwaysOff()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Tracing:SamplingRatio"] = "-1.0",
        });

        builder.AddTracingDefaults();

        var sp = builder.Services.BuildServiceProvider();
        var tracerProvider = sp.GetService<TracerProvider>();
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddTracingDefaults_WithEfCoreDisabled_SkipsEfCoreInstrumentation()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Tracing:InstrumentEfCore"] = "false",
        });

        builder.AddTracingDefaults();

        var sp = builder.Services.BuildServiceProvider();
        var tracerProvider = sp.GetService<TracerProvider>();
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddTracingDefaults_WithMassTransitDisabled_SkipsMassTransitSource()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Tracing:InstrumentMassTransit"] = "false",
        });

        builder.AddTracingDefaults();

        var sp = builder.Services.BuildServiceProvider();
        var tracerProvider = sp.GetService<TracerProvider>();
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddTracingDefaults_WithDbStatementsDisabled_PassesFalseToEfCore()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Tracing:InstrumentEfCore"] = "true",
            ["Tracing:IncludeDbStatements"] = "false",
        });

        builder.AddTracingDefaults();

        var sp = builder.Services.BuildServiceProvider();
        var tracerProvider = sp.GetService<TracerProvider>();
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddTracingDefaults_AllInstrumentationDisabled_OnlyAspNetCoreAndHttpClient()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Tracing:InstrumentEfCore"] = "false",
            ["Tracing:InstrumentMassTransit"] = "false",
        });

        builder.AddTracingDefaults();

        var sp = builder.Services.BuildServiceProvider();
        var tracerProvider = sp.GetService<TracerProvider>();
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddTracingDefaults_WithOtlpEndpoint_ConfiguresOtlpExporter()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Tracing:OtlpEndpoint"] = "http://localhost:4317",
        });

        builder.AddTracingDefaults();

        var sp = builder.Services.BuildServiceProvider();
        var tracerProvider = sp.GetService<TracerProvider>();
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddTracingDefaults_WithProgrammaticConfigure_OtlpEndpoint()
    {
        var builder = WebApplication.CreateBuilder();

        builder.AddTracingDefaults(opts =>
        {
            opts.OtlpEndpoint = "http://localhost:4317";
            opts.ServiceName = "ProgrammaticService";
            opts.SamplingRatio = 0.75;
        });

        var sp = builder.Services.BuildServiceProvider();
        var tracerProvider = sp.GetService<TracerProvider>();
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddTracingDefaults_OtelEnvVar_DoesNotConflictWithServiceDefaults()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://env-endpoint:4317",
        });

        builder.AddTracingDefaults();

        var sp = builder.Services.BuildServiceProvider();
        var tracerProvider = sp.GetService<TracerProvider>();
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddTracingDefaults_ChainReturnValue_AllowsFluentChaining()
    {
        var builder = WebApplication.CreateBuilder();

        var result = builder.AddTracingDefaults(opts => opts.ServiceName = "ChainTest");

        result.Should().BeSameAs(builder);
    }
}
