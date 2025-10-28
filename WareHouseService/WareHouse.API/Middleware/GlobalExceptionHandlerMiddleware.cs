using System.Text.Json;
using Microsoft.AspNetCore.Http;
using WareHouse.Domain.Exceptions;

namespace WareHouse.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
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
            _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Success = false,
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case NotFoundException notFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.ErrorCode = "NOT_FOUND";
                response.Message = notFoundException.Message;
                break;

            // ValidationException должен быть ДО DomainException, так как он наследуется от него
            case ValidationException validationException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.ErrorCode = "VALIDATION_ERROR";
                response.Message = "Validation failed";
                response.Errors = validationException.Errors;
                break;

            case DomainException domainException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.ErrorCode = "DOMAIN_ERROR";
                response.Message = domainException.Message;
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.ErrorCode = "INTERNAL_ERROR";
                response.Message = _env.IsDevelopment() ? exception.Message : "An internal server error occurred";
                break;
        }

        if (_env.IsDevelopment())
        {
            response.StackTrace = exception.StackTrace;
            response.InnerException = exception.InnerException?.Message;
        }

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(response, options);

        await context.Response.WriteAsync(json);
    }
}

public class ErrorResponse
{
    public bool Success { get; set; }
    public string ErrorCode { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string[]> Errors { get; set; }
    public string StackTrace { get; set; }
    public string InnerException { get; set; }
}

public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}