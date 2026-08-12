namespace ABCRetail.Models;

/// <summary>
/// Represents metadata information for a blob item stored in Azure Blob Storage.
/// Used for displaying blob details in the UI.
/// </summary>
public class BlobItemInfo
{
    /// <summary>
    /// The unique name of the blob in the container.
    /// </summary>
    public string BlobName { get; set; } = string.Empty;

    /// <summary>
    /// The original filename as uploaded by the user.
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// The MIME content type of the blob (e.g., "image/jpeg", "application/pdf").
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// The size of the blob in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// The timestamp when the blob was uploaded.
    /// </summary>
    public DateTimeOffset? UploadTimestamp { get; set; }

    /// <summary>
    /// The public URL to access the blob.
    /// </summary>
    public string Url { get; set; } = string.Empty;
}
