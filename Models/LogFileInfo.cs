namespace ABCRetail.Models;

/// <summary>
/// Represents metadata information for a log file stored in Azure Files.
/// Used for displaying log file details in the UI.
/// </summary>
public class LogFileInfo
{
    /// <summary>
    /// The name of the log file.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// The size of the log file in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// The timestamp when the log file was last modified.
    /// </summary>
    public DateTimeOffset? LastModified { get; set; }
}
