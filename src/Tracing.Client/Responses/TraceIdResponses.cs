using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Tracing.Client.Responses;

/// <summary>
/// Where every response carries the 32-character W3C trace id of its request.
/// </summary>
public static class TraceIdResponses
{
    /// <summary>
    /// Response header holding the trace id.
    /// </summary>
    public const string HeaderName = "X-Trace-Id";

    /// <summary>
    /// ProblemDetails extension holding the trace id.
    /// </summary>
    public const string ProblemDetailsKey = "traceId";

    internal static void Register(IServiceCollection services)
    {
        services.AddTransient<IStartupFilter, TraceIdResponseStartupFilter>();
        services.PostConfigure<ProblemDetailsOptions>(options =>
        {
            var customize = options.CustomizeProblemDetails;
            options.CustomizeProblemDetails = context =>
            {
                customize?.Invoke(context);
                AddTraceId(context.ProblemDetails.Extensions);
            };
        });
    }

    internal static string? CurrentTraceId() => Activity.Current?.TraceId.ToHexString();

    private static void AddTraceId(IDictionary<string, object?> extensions)
    {
        var traceId = CurrentTraceId();
        if (traceId is null) return;

        extensions[ProblemDetailsKey] = traceId;
    }
}
