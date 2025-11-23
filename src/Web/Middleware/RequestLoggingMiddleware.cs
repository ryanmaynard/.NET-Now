using System.Diagnostics;

namespace Web.Middleware;

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
        var sw = Stopwatch.StartNew();
        var correlationId = context.TraceIdentifier;

        try
        {
            _logger.LogInformation(
                "HTTP {Method} {Path} started. CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                correlationId);

            await _next(context);

            sw.Stop();

            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms. CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                correlationId);
        }
        catch (Exception)
        {
            sw.Stop();
            _logger.LogError(
                "HTTP {Method} {Path} failed in {ElapsedMs}ms. CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                sw.ElapsedMilliseconds,
                correlationId);
            throw;
        }
    }
}
