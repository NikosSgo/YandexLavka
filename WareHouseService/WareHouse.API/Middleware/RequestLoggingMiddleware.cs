namespace WareHouse.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;

        // Логируем входящий запрос
        _logger.LogInformation("Incoming request: {Method} {Path}",
            context.Request.Method, context.Request.Path);

        // Продолжаем выполнение pipeline
        await _next(context);

        var duration = DateTime.UtcNow - startTime;

        // Логируем ответ
        _logger.LogInformation("Request completed: {Method} {Path} - {StatusCode} in {Duration}ms",
            context.Request.Method, context.Request.Path, context.Response.StatusCode, duration.TotalMilliseconds);
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}