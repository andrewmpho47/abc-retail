using ABCRetail.Models;

namespace ABCRetail.Services.Interfaces;

/// <summary>
/// Defines operations for Azure Blob Storage interactions for images and multimedia files.
/// </summary>
public interface IBlobStorageService
{
    // Image Operations
    
    /// <summary>
    /// Uploads an image file to Azure Blob Storage with metadata.
    /// </summary>
    /// <param name="fileStream">The stream containing the image data.</param>
    /// <param name="fileName">The original filename of the image.</param>
    /// <param name="contentType">The MIME content type of the image.</param>
    /// <returns>The unique blob name assigned to the uploaded image.</returns>
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType);
    
    /// <summary>
    /// Retrieves all images from the image container with their metadata.
    /// </summary>
    /// <returns>A collection of blob metadata for all images.</returns>
    Task<IEnumerable<BlobItemInfo>> GetAllImagesAsync();
    
    /// <summary>
    /// Downloads an image from Azure Blob Storage.
    /// </summary>
    /// <param name="blobName">The unique name of the blob to download.</param>
    /// <returns>A stream containing the image data.</returns>
    Task<Stream> DownloadImageAsync(string blobName);
    
    /// <summary>
    /// Deletes an image from Azure Blob Storage.
    /// </summary>
    /// <param name="blobName">The unique name of the blob to delete.</param>
    Task DeleteImageAsync(string blobName);
    
    /// <summary>
    /// Gets the public URL for an image blob.
    /// </summary>
    /// <param name="blobName">The unique name of the blob.</param>
    /// <returns>The URL to access the blob.</returns>
    Task<string> GetImageUrlAsync(string blobName);
    
    // Multimedia Operations
    
    /// <summary>
    /// Uploads a multimedia file to Azure Blob Storage with metadata.
    /// </summary>
    /// <param name="fileStream">The stream containing the multimedia data.</param>
    /// <param name="fileName">The original filename of the multimedia file.</param>
    /// <param name="contentType">The MIME content type of the multimedia file.</param>
    /// <returns>The unique blob name assigned to the uploaded multimedia file.</returns>
    Task<string> UploadMultimediaAsync(Stream fileStream, string fileName, string contentType);
    
    /// <summary>
    /// Retrieves all multimedia files from the multimedia container with their metadata.
    /// </summary>
    /// <returns>A collection of blob metadata for all multimedia files.</returns>
    Task<IEnumerable<BlobItemInfo>> GetAllMultimediaAsync();
    
    /// <summary>
    /// Downloads a multimedia file from Azure Blob Storage.
    /// </summary>
    /// <param name="blobName">The unique name of the blob to download.</param>
    /// <returns>A stream containing the multimedia data.</returns>
    Task<Stream> DownloadMultimediaAsync(string blobName);
    
    /// <summary>
    /// Deletes a multimedia file from Azure Blob Storage.
    /// </summary>
    /// <param name="blobName">The unique name of the blob to delete.</param>
    Task DeleteMultimediaAsync(string blobName);
}
