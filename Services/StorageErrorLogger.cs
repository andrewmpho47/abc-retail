using ABCRetail.Services.Interfaces;

namespace ABCRetail.Services;

/// <summary>
/// Provides centralized error logging for storage operations to Azure Files.
/// </summary>
public class StorageErrorLogger : IStorageErrorLogger
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<StorageErrorLogger> _logger;
    private const string StorageErrorLogFileName = "storage-errors.log";

    public StorageErrorLogger(
        IFileStorageService fileStorageService,
        ILogger<StorageErrorLogger> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task LogStorageErrorAsync(
        string operationType,
        string serviceName,
        Exception exception,
        string? additionalContext = null)
    {
        var logEntry = FormatLogEntry(operationType, serviceName, exception, additionalContext);
        
        try
        {
            await _fileStorageService.AppendToLogFileAsync(StorageErrorLogFileName, logEntry);
        }
        catch (Exception logEx)
        {
            // If logging to Azure Files fails, log to the standard logger
            // to avoid losing the error information
            _logger.LogError(logEx, 
                "Failed to log storage error to Azure Files. Original error: {OperationType} in {ServiceName} - {Message}",
                operationType, serviceName, exception.Message);
        }
    }

    /// <inheritdoc />
    public async Task LogStorageOperationAsync(
        string operationType,
        string serviceName,
        string message,
        bool isSuccess = true)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff zzz");
        var status = isSuccess ? "SUCCESS" : "FAILURE";
        
        var logEntry = $"""
            [{timestamp}] [{status}] {serviceName} - {operationType}: {message}

            """;
        
        try
        {
            await _fileStorageService.AppendToLogFileAsync(StorageErrorLogFileName, logEntry);
        }
        catch (Exception logEx)
        {
            _logger.LogWarning(logEx, 
                "Failed to log storage operation to Azure Files: {OperationType} in {ServiceName}",
                operationType, serviceName);
        }
    }

    private static string FormatLogEntry(
        string operationType,
        string serviceName,
        Exception exception,
        string? additionalContext)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff zzz");
        var exceptionType = exception.GetType().FullName;
        var message = exception.Message;
        var stackTrace = exception.StackTrace ?? "No stack trace available";
        var innerException = exception.InnerException?.Message ?? "None";

        var contextLine = string.IsNullOrEmpty(additionalContext)
            ? string.Empty
            : $"Additional Context: {additionalContext}\n";

        var logEntry = $"""
            ========================================
            Timestamp: {timestamp}
            Service: {serviceName}
            Operation: {operationType}
            Exception Type: {exceptionType}
            Message: {message}
            Inner Exception: {innerException}
            {contextLine}Stack Trace:
            {stackTrace}
            ========================================

            """;

        return logEntry;
    }
}
