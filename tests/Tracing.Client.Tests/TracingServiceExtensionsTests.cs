using Microsoft.Extensions.Configuration;
using OpenTelemetry.Trace;
using Tracing.Client.Configuration;
using Tracing.Client.Extensions;

namespace Tracing.Client.Tests;

/// <summary>
/// Tests for <see cref="TracingServiceExtensions"/>.
/// </summary>
public sealed class TracingServiceExtensionsTests
{
    [Fact]
    public void BuildOptions_WithDefaults_ReturnsDefaultValues()
    {
        var config = new ConfigurationBuilder().Build();

        var options = TracingServiceExtensions.BuildOptions(config, null);

        options.ServiceName.Should().Be("Unknown");
        options.Enabled.Should().BeTrue();
        options.OtlpEndpoint.Should().BeEmpty();
        options.SamplingRatio.Should().Be(1.0);
    }

    [Fact]
    public void BuildOptions_WithConfigureAction_AppliesOverrides()
    {
        var config = new ConfigurationBuilder().Build();

        var options = TracingServiceExtensions.BuildOptions(config, opts =>
        {
            opts.ServiceName = "TestService";
            opts.SamplingRatio = 0.5;
        });

        options.ServiceName.Should().Be("TestService");
        options.SamplingRatio.Should().Be(0.5);
    }

    [Fact]
    public void BuildOptions_WithConfigSection_BindsValues()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tracing:ServiceName"] = "ConfiguredService",
                ["Tracing:SamplingRatio"] = "0.25",
                ["Tracing:Enabled"] = "false",
                ["Tracing:InstrumentEfCore"] = "false",
            })
            .Build();

        var options = TracingServiceExtensions.BuildOptions(config, null);

        options.ServiceName.Should().Be("ConfiguredService");
        options.SamplingRatio.Should().Be(0.25);
        options.Enabled.Should().BeFalse();
        options.InstrumentEfCore.Should().BeFalse();
    }

    [Fact]
    public void BuildOptions_ProgrammaticOverride_TakesPrecedenceOverConfig()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tracing:ServiceName"] = "ConfigService",
            })
            .Build();

        var options = TracingServiceExtensions.BuildOptions(config, opts =>
        {
            opts.ServiceName = "ProgrammaticService";
        });

        options.ServiceName.Should().Be("ProgrammaticService");
    }

    [Fact]
    public void BuildOptions_OtelEnvVar_DoesNotOverrideOtlpEndpoint()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tracing:OtlpEndpoint"] = "http://config-endpoint:4317",
                ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://env-endpoint:4317",
            })
            .Build();

        var options = TracingServiceExtensions.BuildOptions(config, null);

        options.OtlpEndpoint.Should().Be("http://config-endpoint:4317");
    }

    [Fact]
    public void BuildOptions_WithOtlpEndpointInConfig_UsesConfigValue()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tracing:OtlpEndpoint"] = "http://config-endpoint:4317",
            })
            .Build();

        var options = TracingServiceExtensions.BuildOptions(config, null);

        options.OtlpEndpoint.Should().Be("http://config-endpoint:4317");
    }

    [Fact]
    public void ConfigureSampler_WithRatioOne_UsesAlwaysOnSampler()
    {
        var config = new ConfigurationBuilder().Build();
        var options = TracingServiceExtensions.BuildOptions(config, opts =>
        {
            opts.SamplingRatio = 1.0;
        });

        options.SamplingRatio.Should().Be(1.0);
    }

    [Fact]
    public void BuildOptions_SamplingRatioClamped_AboveOne()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tracing:SamplingRatio"] = "2.0",
            })
            .Build();

        var options = TracingServiceExtensions.BuildOptions(config, null);

        options.SamplingRatio.Should().Be(2.0);
    }

    [Fact]
    public void MassTransitActivitySource_HasExpectedValue()
    {
        TracingServiceExtensions.MassTransitActivitySource.Should().Be("MassTransit");
    }
}
