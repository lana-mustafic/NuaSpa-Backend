using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.Exceptions;

namespace NuaSpa.Api.Middleware;

/// <summary>
/// Centralizovano hvatanje izuzetaka, logovanje i ProblemDetails odgovor.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
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

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var correlationId = context.Items[RequestLoggingMiddleware.CorrelationIdItemKey]?.ToString()
            ?? context.TraceIdentifier;

        var (status, title, detail) = MapException(ex);

        var userId = context.User.Identity?.IsAuthenticated == true
            ? context.User.Identity?.Name
            : null;

        _logger.LogError(
            ex,
            "Unhandled exception | CorrelationId={CorrelationId} {Method} {Path} User={User} Status={Status} Title={Title}",
            correlationId,
            context.Request.Method,
            context.Request.Path.Value,
            userId ?? "(anonymous)",
            (int)status,
            title);

        if (context.Response.HasStarted)
        {
            _logger.LogWarning(
                ex,
                "Response already started; cannot write error body | CorrelationId={CorrelationId}",
                correlationId);
            return;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;

        var clientDetail = ResolveClientDetail(ex, detail);

        var problem = new ProblemDetails
        {
            Status = (int)status,
            Title = title,
            Detail = clientDetail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{(int)status}",
        };
        problem.Extensions["correlationId"] = correlationId;
        if (_env.IsDevelopment())
        {
            problem.Extensions["exceptionType"] = ex.GetType().FullName;
            problem.Extensions["stackTrace"] = ex.StackTrace;
        }

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
        await context.Response.WriteAsync(json);
    }

    private static string ResolveClientDetail(Exception ex, string fallbackDetail)
    {
        if (ex is BusinessRuleException or ConflictException or NotFoundException
            or ForbiddenException or UnauthorizedException)
        {
            var message = ex.Message?.Trim();
            if (!string.IsNullOrWhiteSpace(message))
            {
                return message;
            }
        }

        return fallbackDetail;
    }

    private static (HttpStatusCode status, string title, string clientDetail) MapException(Exception ex)
    {
        if (ex is DbUpdateException dbEx)
        {
            var sql = dbEx.InnerException as SqlException;
            if (sql?.Number is 547 or 2627 or 2601)
            {
                return (
                    HttpStatusCode.Conflict,
                    "Referencijalni integritet.",
                    "Zapis se ne može obrisati ili izmijeniti jer ga koriste drugi podaci u bazi.");
            }
        }

        return ex switch
        {
            NotFoundException or KeyNotFoundException =>
                (HttpStatusCode.NotFound, "Resurs nije pronađen.", "Traženi resurs ne postoji."),
            BusinessRuleException or ArgumentException or InvalidOperationException =>
                (HttpStatusCode.BadRequest, "Neispravan zahtjev.", "Zahtjev nije mogao biti obraden."),
            ConflictException =>
                (HttpStatusCode.Conflict, "Konflikt.", "Operacija nije dozvoljena u trenutnom stanju."),
            ForbiddenException =>
                (HttpStatusCode.Forbidden, "Pristup odbijen.", "Nemate dozvolu za ovu operaciju."),
            UnauthorizedException =>
                (HttpStatusCode.Unauthorized, "Neautorizovan zahtjev.", "Provjerite vjerodajnice."),
            UnauthorizedAccessException =>
                (HttpStatusCode.Forbidden, "Pristup odbijen.", "Nemate dozvolu za ovu operaciju."),
            _ =>
                (HttpStatusCode.InternalServerError, "Greška servera.", "Došlo je do neočekivane greške."),
        };
    }
}
