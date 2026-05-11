using System.Security.Claims;
using ClinicBackend.Services.Interfaces;

namespace ClinicBackend.Middleware;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        await _next(context);

        // Log non-auth requests that resulted in errors
        if (context.Response.StatusCode >= 400)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var path = context.Request.Path.Value;
                var method = context.Request.Method;
                await auditService.LogAsync(Guid.Parse(userId), null, $"HTTP_{method}_{context.Response.StatusCode}",
                    $"Path: {path}");
            }
        }
    }
}