using ABCRetail.Models;

namespace ABCRetail.Services.Interfaces;

/// <summary>
/// Defines operations for Azure Files interactions for application log files.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Creates a new log file with the specified content.
    /// If a file with the same name exists, it will be overwritten.
    /// </summary>
    /// <param name="fileName">The name of the log file to create.</param>
    /// <param name="content">The content to write to the file.</param>
    Task CreateLogFileAsync(string fileName, string content);

    /// <summary>
    /// Retrieves all log files from the Azure File Share with their metadata.
    /// </summary>
    /// <returns>A collection of log file metadata.</returns>
    Task<IEnumerable<LogFileInfo>> GetAllLogFilesAsync();

    /// <summary>
    /// Retrieves the content of a specific log file.
    /// </summary>
    /// <param name="fileName">The name of the log file to read.</param>
    /// <returns>The content of the log file as a string.</returns>
    Task<string> GetLogFileContentAsync(string fileName);

    /// <summary>
    /// Downloads a log file as a stream.
    /// </summary>
    /// <param name="fileName">The name of the log file to download.</param>
    /// <returns>A stream containing the file data.</returns>
    Task<Stream> DownloadLogFileAsync(string fileName);

    /// <summary>
    /// Deletes a log file from the Azure File Share.
    /// </summary>
    /// <param name="fileName">The name of the log file to delete.</param>
    Task DeleteLogFileAsync(string fileName);

    /// <summary>
    /// Appends content to an existing log file, or creates a new file if it doesn't exist.
    /// Useful for error logging operations.
    /// </summary>
    /// <param name="fileName">The name of the log file to append to.</param>
    /// <param name="content">The content to append to the file.</param>
    Task AppendToLogFileAsync(string fileName, string content);
}
