using System.Diagnostics;

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

        _logger.LogInformation("{Method} {Path} {StatusCode} {ElapsedMs}ms | IP: {IP} | Agent: {Agent}",
            method, path, context.Response.StatusCode, sw.ElapsedMilliseconds, ipAddress, userAgent);
    }
}
