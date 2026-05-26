using System.Diagnostics;
using System.Security.Claims;

namespace NuaSpa.Api.Middleware;

/// <summary>
/// Dodaje Correlation-Id zaglavlje i loguje svaki HTTP zahtjev (reprodukcija problema).
/// </summary>
public sealed class RequestLoggingMiddleware
{
    public const string CorrelationIdHeader = "X-Correlation-Id";
    public const string CorrelationIdItemKey = "CorrelationId";

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.Items[CorrelationIdItemKey] = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        var sw = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "/";
        var query = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");
        var roles = string.Join(
            ",",
            context.User.FindAll(ClaimTypes.Role).Select(c => c.Value));

        _logger.LogInformation(
            "HTTP {Method} {Path}{Query} started | CorrelationId={CorrelationId} UserId={UserId} Roles={Roles}",
            method,
            path,
            query,
            correlationId,
            userId ?? "(anonymous)",
            string.IsNullOrEmpty(roles) ? "(none)" : roles);

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation(
                "HTTP {Method} {Path} completed {StatusCode} in {ElapsedMs}ms | CorrelationId={CorrelationId}",
                method,
                path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                correlationId);
        }
    }
}
