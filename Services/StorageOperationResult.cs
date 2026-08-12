namespace ABCRetail.Services;

/// <summary>
/// Represents the result of an Azure Storage operation with optional data and error information.
/// </summary>
/// <typeparam name="T">The type of data returned from the operation.</typeparam>
public class StorageOperationResult<T>
{
    /// <summary>
    /// Indicates whether the operation completed successfully.
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// The data returned from the operation (if successful).
    /// </summary>
    public T? Data { get; set; }
    
    /// <summary>
    /// A user-friendly error message (if the operation failed).
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// The error code from Azure Storage (if applicable).
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Creates a successful result with the specified data.
    /// </summary>
    /// <param name="data">The data to return.</param>
    /// <returns>A successful StorageOperationResult.</returns>
    public static StorageOperationResult<T> SuccessResult(T data) =>
        new() { Success = true, Data = data };

    /// <summary>
    /// Creates a failed result with the specified error information.
    /// </summary>
    /// <param name="errorMessage">The user-friendly error message.</param>
    /// <param name="errorCode">The optional error code.</param>
    /// <returns>A failed StorageOperationResult.</returns>
    public static StorageOperationResult<T> FailureResult(string errorMessage, string? errorCode = null) =>
        new() { Success = false, ErrorMessage = errorMessage, ErrorCode = errorCode };
}

/// <summary>
/// Represents the result of an Azure Storage operation without return data.
/// </summary>
public class StorageOperationResult
{
    /// <summary>
    /// Indicates whether the operation completed successfully.
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// A user-friendly error message (if the operation failed).
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// The error code from Azure Storage (if applicable).
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful StorageOperationResult.</returns>
    public static StorageOperationResult SuccessResult() =>
        new() { Success = true };

    /// <summary>
    /// Creates a failed result with the specified error information.
    /// </summary>
    /// <param name="errorMessage">The user-friendly error message.</param>
    /// <param name="errorCode">The optional error code.</param>
    /// <returns>A failed StorageOperationResult.</returns>
    public static StorageOperationResult FailureResult(string errorMessage, string? errorCode = null) =>
        new() { Success = false, ErrorMessage = errorMessage, ErrorCode = errorCode };
}
