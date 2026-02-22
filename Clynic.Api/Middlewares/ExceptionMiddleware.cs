using System.Security.Claims;
using System.Text.Json;
using FluentValidation;

namespace Clynic.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
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
            var request = context.Request;
            var endpoint = $"{request.Method} {request.Path}";
            var queryString = request.QueryString.ToString();
            var traceId = context.TraceIdentifier;

            var userId = context.User?.Claims?
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                ?.Value;

            var userName = context.User?.Identity?.Name;

            _logger.LogError(ex,
                @"Unhandled exception occurred
                  Endpoint: {Endpoint}
                  Query: {Query}
                  UserId: {UserId}
                  UserName: {UserName}
                  TraceId: {TraceId}
                  Message: {Message}",
                endpoint,
                queryString,
                userId ?? "Anonymous",
                userName ?? "Anonymous",
                traceId,
                ex.Message
            );

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = GetStatusCode(ex);

            var response = new ErrorResponse
            {
                StatusCode = context.Response.StatusCode,
                Message = GetClientMessage(ex),
                TraceId = traceId,
                Timestamp = DateTime.UtcNow,
                Errors = GetValidationErrors(ex)
            };

            var json = JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            await context.Response.WriteAsync(json);
        }

        private static int GetStatusCode(Exception ex)
        {
            return ex switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                ArgumentException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        private static string GetClientMessage(Exception ex)
        {
            return ex switch
            {
                ValidationException => "Errores de validación.",
                KeyNotFoundException => "No se encontró el recurso solicitado.",
                UnauthorizedAccessException => "No autorizado.",
                ArgumentException => ex.Message,
                InvalidOperationException => ex.Message,
                _ => "Ocurrió un error inesperado en el servidor."
            };
        }

        private static string[]? GetValidationErrors(Exception ex)
        {
            if (ex is not ValidationException validationException)
            {
                return null;
            }

            return validationException.Errors
                .Select(e => e.ErrorMessage)
                .ToArray();
        }

        private sealed class ErrorResponse
        {
            public int StatusCode { get; init; }

            public string Message { get; init; } = string.Empty;

            public string TraceId { get; init; } = string.Empty;

            public DateTime Timestamp { get; init; }

            public string[]? Errors { get; init; }
        }
    }
}
