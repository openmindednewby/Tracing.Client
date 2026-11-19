using Tracing.Client.Configuration;

namespace Tracing.Client.Tests;

/// <summary>
/// Tests for <see cref="TracingOptions"/> default values and property behavior.
/// </summary>
public sealed class TracingOptionsTests
{
    [Fact]
    public void ServiceName_WithDefaults_IsUnknown()
    {
        var options = new TracingOptions();

        options.ServiceName.Should().Be("Unknown");
    }

    [Fact]
    public void Enabled_WithDefaults_IsTrue()
    {
        var options = new TracingOptions();

        options.Enabled.Should().BeTrue();
    }

    [Fact]
    public void OtlpEndpoint_WithDefaults_IsEmpty()
    {
        var options = new TracingOptions();

        options.OtlpEndpoint.Should().BeEmpty();
    }

    [Fact]
    public void SamplingRatio_WithDefaults_IsOne()
    {
        var options = new TracingOptions();

        options.SamplingRatio.Should().Be(1.0);
    }

    [Fact]
    public void InstrumentEfCore_WithDefaults_IsTrue()
    {
        var options = new TracingOptions();

        options.InstrumentEfCore.Should().BeTrue();
    }

    [Fact]
    public void InstrumentMassTransit_WithDefaults_IsTrue()
    {
        var options = new TracingOptions();

        options.InstrumentMassTransit.Should().BeTrue();
    }

    [Fact]
    public void IncludeDbStatements_WithDefaults_IsTrue()
    {
        var options = new TracingOptions();

        options.IncludeDbStatements.Should().BeTrue();
    }

    [Fact]
    public void SectionName_WhenRead_IsTracing()
    {
        TracingOptions.SectionName.Should().Be("Tracing");
    }
}
