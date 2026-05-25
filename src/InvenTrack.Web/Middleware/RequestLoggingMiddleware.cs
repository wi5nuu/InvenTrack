using System.Diagnostics;
using Serilog;

namespace InvenTrack.Web.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/css") || path.StartsWith("/js") || path.StartsWith("/lib"))
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        var method = context.Request.Method;

        await _next(context);

        sw.Stop();

        Log.ForContext("RequestMethod", method)
           .ForContext("RequestPath", path)
           .ForContext("StatusCode", context.Response.StatusCode)
           .ForContext("ElapsedMs", sw.ElapsedMilliseconds)
           .Information("{Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
               method, path, context.Response.StatusCode, sw.ElapsedMilliseconds);
    }
}
