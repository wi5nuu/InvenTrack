using System.Net;
using InvenTrack.Domain.Exceptions;

namespace InvenTrack.Web.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in Web UI");
            var message = _env.IsDevelopment() ? ex.Message : "An internal server error occurred.";

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.Redirect($"/Home/Error?message={Uri.EscapeDataString(message)}");
        }
    }
}
