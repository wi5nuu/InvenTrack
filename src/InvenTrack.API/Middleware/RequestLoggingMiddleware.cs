using System.Diagnostics;
using Serilog;

namespace InvenTrack.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // SECURITY A09: Comprehensive logging
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Skip health checks
        if (path.Equals("/health", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        var method = context.Request.Method;
        var queryString = context.Request.QueryString.Value;
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context.Request.Headers.UserAgent.ToString();

        await _next(context);

        sw.Stop();

        Log.ForContext("RequestMethod", method)
           .ForContext("RequestPath", path)
           .ForContext("QueryString", queryString)
           .ForContext("IpAddress", ipAddress)
           .ForContext("UserAgent", userAgent)
           .ForContext("StatusCode", context.Response.StatusCode)
           .ForContext("ElapsedMs", sw.ElapsedMilliseconds)
           .Information("{Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
               method, path, context.Response.StatusCode, sw.ElapsedMilliseconds);
    }
}
