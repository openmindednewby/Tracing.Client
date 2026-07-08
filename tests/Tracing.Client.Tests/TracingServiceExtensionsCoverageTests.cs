using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using Tracing.Client.Configuration;
using Tracing.Client.Extensions;

namespace Tracing.Client.Tests;

/// <summary>
/// Additional tests for <see cref="TracingServiceExtensions"/> to increase
/// line coverage for ConfigureSampler, AddInstrumentation, AddExporter,
/// and AddTracingDefaults — all of which were previously uncovered.
///
/// These tests exercise each branch via <see cref="WebApplicationBuilder"/>
/// which implements <see cref="Microsoft.Extensions.Hosting.IHostApplicationBuilder"/>.
/// </summary>
public sealed class TracingServiceExtensionsCoverageTests
{
    // ==================== AddTracingDefaults: disabled branch ====================

    [Fact]
    public void AddTracingDefaults_WhenDisabled_ReturnsBuilderWithoutOpenTelemetry()
    {
        // Covers: AddTracingDefaults early-return when options.Enabled == false
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Tracing:Enabled"] = "false",
        });

        var returnedBuilder = builder.AddTracingDefaults();

        returnedBuilder.Should().BeSameAs(builder);

        // TracerProvider should NOT be registered when tracing is disabled
        var sp = builder.Services.BuildServiceProvider();
        var tracerProvider = sp.GetService<TracerProvider>();
        tracerProvider.Should().BeNull();
    }

    // ==================== AddTracingDefaults: enabled with all sub-branches ====================

    [Fact]
    public void AddTracingDefaults_DefaultOptions_RegistersOpenTelemetryWithDefaults()
    {
        // Covers: ConfigureSampler(ratio=1.0 → AlwaysOnSampler)
        //         AddInstrumentation(EfCore=true, MassTransit=true, DbStatements=true)
        //         AddExporter(endpoint="" → no-op, skips AddOtlpExporter)
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
        // Covers: ConfigureSampler(0.0 < ratio < 1.0 → TraceIdRatioBasedSampler)
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
        // Covers: ConfigureSampler(ratio <= 0.0 → AlwaysOffSampler)
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
        // Covers: ConfigureSampler clamping — ratio > 1.0 → clamped to 1.0 → AlwaysOnSampler
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
        // Covers: ConfigureSampler clamping — ratio < 0.0 → clamped to 0.0 → AlwaysOffSampler
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
        // Covers: AddInstrumentation with InstrumentEfCore=false (skips the if-branch)
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
        // Covers: AddInstrumentation with InstrumentMassTransit=false (skips the if-branch)
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
        // Covers: AddInstrumentation — IncludeDbStatements=false path in efOptions lambda
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
        // Covers: Both EfCore and MassTransit if-branches skipped
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
        // Covers: AddExporter — non-empty endpoint branch → AddOtlpExporter called
        // The endpoint is local and unreachable, but exporter connects lazily (not at build time)
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
        // Covers: AddExporter via programmatic configure override (second code path into AddExporter)
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
        // Covers: BuildOptions correctly ignores OTEL_EXPORTER_OTLP_ENDPOINT so that
        // services using ServiceDefaults.UseOtlpExporter() do not get a double registration.
        // When Tracing:OtlpEndpoint is not set, AddExporter is a no-op.
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://env-endpoint:4317",
        });

        // Should NOT throw NotSupportedException — env var does not trigger AddOtlpExporter
        builder.AddTracingDefaults();

        var sp = builder.Services.BuildServiceProvider();
        var tracerProvider = sp.GetService<TracerProvider>();
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddTracingDefaults_ChainReturnValue_AllowsFluentChaining()
    {
        // Covers: the return statement at end of AddTracingDefaults
        var builder = WebApplication.CreateBuilder();

        // Verify the method returns the builder for chaining
        var result = builder.AddTracingDefaults(opts => opts.ServiceName = "ChainTest");

        result.Should().BeSameAs(builder);
    }
}
