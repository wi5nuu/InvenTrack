using InvenTrack.Shared.Constants;
using Microsoft.Extensions.Caching.Memory;

namespace InvenTrack.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    // SECURITY A07: Rate limiting to prevent abuse
    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (IsIpWhitelisted(ipAddress))
        {
            await _next(context);
            return;
        }

        var limitPerMinute = context.Request.Path.StartsWithSegments("/api")
            ? AppConstants.Limits.ApiRateLimitPerMinute
            : AppConstants.Limits.WebRateLimitPerMinute;

        var cacheKey = $"{AppConstants.CacheKeys.RateLimit}{ipAddress}";
        var counter = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return new RateLimitCounter();
        });

        if (counter != null)
        {
            if (counter.Count >= limitPerMinute)
            {
                context.Response.StatusCode = 429;
                context.Response.Headers["Retry-After"] = "60";
                context.Response.ContentType = "application/json";

                _logger.LogWarning("Rate limit exceeded for IP {IpAddress}", ipAddress);

                await context.Response.WriteAsync(
                    $"{{\"success\":false,\"message\":\"Rate limit exceeded. Try again in 60 seconds.\",\"errorCode\":\"RATE_LIMITED\"}}");
                return;
            }

            counter.Count++;
        }

        await _next(context);
    }

    private static bool IsIpWhitelisted(string ipAddress)
    {
        return ipAddress == "127.0.0.1" || ipAddress == "::1" || ipAddress == "localhost";
    }
}

public class RateLimitCounter
{
    public int Count { get; set; } = 0;
}
