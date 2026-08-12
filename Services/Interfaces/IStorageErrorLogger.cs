namespace ABCRetail.Services.Interfaces;

/// <summary>
/// Defines operations for logging storage errors to Azure Files.
/// </summary>
public interface IStorageErrorLogger
{
    /// <summary>
    /// Logs a storage operation error to Azure Files.
    /// </summary>
    /// <param name="operationType">The type of operation that failed (e.g., "CreateCustomer", "UploadImage").</param>
    /// <param name="serviceName">The name of the service where the error occurred.</param>
    /// <param name="exception">The exception that was thrown.</param>
    /// <param name="additionalContext">Optional additional context information.</param>
    Task LogStorageErrorAsync(
        string operationType,
        string serviceName,
        Exception exception,
        string? additionalContext = null);

    /// <summary>
    /// Logs a storage operation (success or failure) to Azure Files.
    /// </summary>
    /// <param name="operationType">The type of operation.</param>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="message">A descriptive message about the operation.</param>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    Task LogStorageOperationAsync(
        string operationType,
        string serviceName,
        string message,
        bool isSuccess = true);
}
