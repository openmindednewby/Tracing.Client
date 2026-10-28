using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Tracing.Client.Extensions;
using Tracing.Client.Responses;

namespace Tracing.Client.Tests;

public sealed class TraceIdResponseTests
{
    private const string IncomingTraceId = "4bf92f3577b34da6a3ce929d0e0e4736";
    private const string IncomingTraceparent = $"00-{IncomingTraceId}-00f067aa0ba902b7-01";

    [Fact]
    public async Task Request_WithIncomingTraceparent_ReturnsTraceIdHeader()
    {
        await using var app = await StartAppAsync();
        using var request = TracedRequest("/fails");

        using var response = await app.GetTestClient().SendAsync(request);

        response.Headers.GetValues(TraceIdResponses.HeaderName).Should().Equal(IncomingTraceId);
    }

    [Fact]
    public async Task Request_WhenEndpointThrows_ReturnsTraceIdHeader()
    {
        await using var app = await StartAppAsync();
        using var request = TracedRequest("/throws");

        using var response = await app.GetTestClient().SendAsync(request);

        response.Headers.GetValues(TraceIdResponses.HeaderName).Should().Equal(IncomingTraceId);
    }

    [Fact]
    public async Task Request_WhenEndpointReturnsProblem_ReturnsTraceIdInBody()
    {
        await using var app = await StartAppAsync();
        using var request = TracedRequest("/problem");

        using var response = await app.GetTestClient().SendAsync(request);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty(TraceIdResponses.ProblemDetailsKey).GetString().Should().Be(IncomingTraceId);
    }

    private static HttpRequestMessage TracedRequest(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("traceparent", IncomingTraceparent);
        return request;
    }

    private static async Task<WebApplication> StartAppAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddProblemDetails();
        builder.AddTracingDefaults(options => options.ServiceName = "trace-id-tests");
        var app = builder.Build();
        app.UseExceptionHandler();
        app.MapGet("/fails", () => Results.StatusCode(StatusCodes.Status500InternalServerError));
        app.MapGet("/throws", IResult () => throw new InvalidOperationException("boom"));
        app.MapGet("/problem", () => Results.Problem(statusCode: StatusCodes.Status500InternalServerError));
        await app.StartAsync();
        return app;
    }
}
