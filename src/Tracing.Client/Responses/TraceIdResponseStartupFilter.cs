using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Tracing.Client.Responses;

internal sealed class TraceIdResponseStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
        app =>
        {
            app.Use(WriteTraceIdHeader);
            next(app);
        };

    private static Task WriteTraceIdHeader(HttpContext context, RequestDelegate next)
    {
        var traceId = TraceIdResponses.CurrentTraceId();
        if (traceId is null) return next(context);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[TraceIdResponses.HeaderName] = traceId;
            return Task.CompletedTask;
        });
        return next(context);
    }
}
