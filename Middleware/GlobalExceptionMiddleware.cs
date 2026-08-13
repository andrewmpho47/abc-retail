using ABCRetail.Services.Interfaces;

namespace ABCRetail.Middleware;

/// <summary>
/// Middleware that catches unhandled exceptions, logs them to Azure Files,
/// and redirects the user to the error page.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IServiceProvider _serviceProvider;
    private const string ErrorLogFileName = "application-errors.log";

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IServiceProvider serviceProvider)
    {
        _next = next;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request.");
            
            await LogExceptionToAzureFilesAsync(ex, context);
            
            await HandleExceptionAsync(context);
        }
    }

    private async Task LogExceptionToAzureFilesAsync(Exception exception, HttpContext context)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var fileStorageService = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
            var logEntry = FormatLogEntry(exception, context);
            await fileStorageService.AppendToLogFileAsync(ErrorLogFileName, logEntry);
        }
        catch (Exception logEx)
        {
            // If logging to Azure Files fails, log to the standard logger
            // to avoid losing the error information entirely
            _logger.LogError(logEx, "Failed to log exception to Azure Files.");
        }
    }

    private static string FormatLogEntry(Exception exception, HttpContext context)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff zzz");
        var requestPath = context.Request.Path.ToString();
        var requestMethod = context.Request.Method;
        var exceptionType = exception.GetType().FullName;
        var message = exception.Message;
        var stackTrace = exception.StackTrace ?? "No stack trace available";

        var logEntry = $"""
            ========================================
            Timestamp: {timestamp}
            Request Path: {requestMethod} {requestPath}
            Exception Type: {exceptionType}
            Message: {message}
            Stack Trace:
            {stackTrace}
            ========================================

            """;

        return logEntry;
    }

    private static async Task HandleExceptionAsync(HttpContext context)
    {
        // Clear any partial response
        context.Response.Clear();
        
        // Check if this is an API request (accepts JSON)
        var acceptHeader = context.Request.Headers.Accept.ToString();
        if (acceptHeader.Contains("application/json"))
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "An unexpected error occurred.",
                message = "Please try again later or contact support if the problem persists."
            });
        }
        else
        {
            // Redirect to the error page for browser requests
            context.Response.Redirect("/Home/Error");
        }
    }
}

/// <summary>
/// Extension methods for registering the GlobalExceptionMiddleware.
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    /// <summary>
    /// Adds the global exception handling middleware to the application pipeline.
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
