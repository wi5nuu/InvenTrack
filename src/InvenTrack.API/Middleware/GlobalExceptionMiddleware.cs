using System.Net;
using System.Text.Json;
using InvenTrack.Domain.Exceptions;
using InvenTrack.Shared.Wrappers;

namespace InvenTrack.API.Middleware;

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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;
        var response = new ApiResponse<object>();

        // SECURITY: Never expose internal details in production
        switch (exception)
        {
            case NotFoundException notFound:
                response = ApiResponse<object>.Fail(notFound.Message, errorCode: "NOT_FOUND");
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;

            case ValidationException validation:
                response = ApiResponse<object>.Fail(validation.Message, validation.Errors.SelectMany(e => e.Value).ToList(), "VALIDATION_ERROR");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case UnauthorizedException unauthorized:
                response = ApiResponse<object>.Fail(unauthorized.Message, errorCode: "UNAUTHORIZED");
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;

            case ForbiddenException forbidden:
                response = ApiResponse<object>.Fail(forbidden.Message, errorCode: "FORBIDDEN");
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                break;

            default:
                _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", traceId);

                response = ApiResponse<object>.Fail(
                    _env.IsDevelopment() ? exception.Message : "An internal server error occurred.",
                    errorCode: "INTERNAL_ERROR");

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        response.TraceId = traceId;

        context.Response.ContentType = "application/json";
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
