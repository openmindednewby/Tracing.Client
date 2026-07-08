using Tracing.Client.Configuration;

namespace Tracing.Client.Tests;

/// <summary>
/// Tests for <see cref="TracingOptions"/> default values and property behavior.
/// </summary>
public sealed class TracingOptionsTests
{
    [Fact]
    public void Defaults_ServiceName_IsUnknown()
    {
        var options = new TracingOptions();

        options.ServiceName.Should().Be("Unknown");
    }

    [Fact]
    public void Defaults_Enabled_IsTrue()
    {
        var options = new TracingOptions();

        options.Enabled.Should().BeTrue();
    }

    [Fact]
    public void Defaults_OtlpEndpoint_IsEmpty()
    {
        var options = new TracingOptions();

        options.OtlpEndpoint.Should().BeEmpty();
    }

    [Fact]
    public void Defaults_SamplingRatio_IsOne()
    {
        var options = new TracingOptions();

        options.SamplingRatio.Should().Be(1.0);
    }

    [Fact]
    public void Defaults_InstrumentEfCore_IsTrue()
    {
        var options = new TracingOptions();

        options.InstrumentEfCore.Should().BeTrue();
    }

    [Fact]
    public void Defaults_InstrumentMassTransit_IsTrue()
    {
        var options = new TracingOptions();

        options.InstrumentMassTransit.Should().BeTrue();
    }

    [Fact]
    public void Defaults_IncludeDbStatements_IsTrue()
    {
        var options = new TracingOptions();

        options.IncludeDbStatements.Should().BeTrue();
    }

    [Fact]
    public void SectionName_IsTracing()
    {
        TracingOptions.SectionName.Should().Be("Tracing");
    }
}
