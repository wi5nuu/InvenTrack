using System.Security.Claims;
using System.Text;
using InvenTrack.Application.Interfaces;
using InvenTrack.Domain.Entities;

namespace InvenTrack.API.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // SECURITY A09: Audit trail for all data modifications
    public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        if (context.Request.Method is "POST" or "PUT" or "PATCH" or "DELETE")
        {
            var originalBody = context.Request.Body;
            var bodyContent = string.Empty;

            if (context.Request.ContentLength > 0)
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                bodyContent = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var userAgent = context.Request.Headers.UserAgent.ToString();

            await _next(context);

            if (int.TryParse(userIdClaim, out var userId) && userId > 0)
            {
                var path = context.Request.Path.Value ?? "";
                var entityName = path.Split('/').ElementAtOrDefault(3) ?? "Unknown";

                await unitOfWork.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = userId,
                    Action = $"{context.Request.Method} {path}",
                    EntityName = entityName,
                    EntityId = context.Request.RouteValues["id"]?.ToString(),
                    NewValues = bodyContent.Length > 500 ? bodyContent[..500] : bodyContent,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    CreatedAt = DateTime.UtcNow
                });
                await unitOfWork.SaveChangesAsync();
            }
        }
        else
        {
            await _next(context);
        }
    }
}
