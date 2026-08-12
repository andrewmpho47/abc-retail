using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ABCRetail.Models;
using ABCRetail.Services.Interfaces;

namespace ABCRetail.Services;

/// <summary>
/// Implements Azure Blob Storage operations for images and multimedia files.
/// </summary>
public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _imageContainerClient;
    private readonly BlobContainerClient _multimediaContainerClient;

    // Allowed file extensions for validation
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpeg", ".jpg", ".png", ".gif", ".webp"
    };

    private static readonly HashSet<string> AllowedMultimediaExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".mp4", ".docx", ".xlsx"
    };

    // File size limits
    private const long MaxImageSizeBytes = 10 * 1024 * 1024; // 10 MB
    private const long MaxMultimediaSizeBytes = 100 * 1024 * 1024; // 100 MB

    // Metadata keys
    private const string MetadataOriginalFileName = "OriginalFileName";
    private const string MetadataUploadTimestamp = "UploadTimestamp";
    private const string MetadataContentType = "ContentType";

    /// <summary>
    /// Initializes a new instance of the BlobStorageService.
    /// </summary>
    /// <param name="settings">The Azure Storage settings containing connection strings and container names.</param>
    public BlobStorageService(AzureStorageSettings settings)
    {
        var connectionString = settings.GetBlobConnectionString();
        
        // Create BlobServiceClient
        var blobServiceClient = new BlobServiceClient(connectionString);
        
        // Get or create the image container (private access - public access not allowed by policy)
        _imageContainerClient = blobServiceClient.GetBlobContainerClient(settings.ImageContainerName);
        _imageContainerClient.CreateIfNotExists();
        
        // Get or create the multimedia container (private access)
        _multimediaContainerClient = blobServiceClient.GetBlobContainerClient(settings.MultimediaContainerName);
        _multimediaContainerClient.CreateIfNotExists();
    }

    #region Image Operations

    /// <inheritdoc />
    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType)
    {
        // Validate file type
        var extension = Path.GetExtension(fileName);
        if (!IsValidImageType(extension))
        {
            throw new InvalidOperationException($"Invalid image file type. Allowed types: {string.Join(", ", AllowedImageExtensions)}");
        }

        // Validate file size
        if (fileStream.Length > MaxImageSizeBytes)
        {
            throw new InvalidOperationException($"Image file size exceeds the maximum allowed size of {MaxImageSizeBytes / (1024 * 1024)} MB.");
        }

        // Generate unique blob name
        var blobName = GenerateUniqueBlobName(fileName);
        
        try
        {
            var blobClient = _imageContainerClient.GetBlobClient(blobName);
            
            // Prepare metadata
            var metadata = new Dictionary<string, string>
            {
                { MetadataOriginalFileName, fileName },
                { MetadataUploadTimestamp, DateTimeOffset.UtcNow.ToString("O") },
                { MetadataContentType, contentType }
            };

            // Upload with headers and metadata
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                },
                Metadata = metadata
            };

            await blobClient.UploadAsync(fileStream, uploadOptions);
            
            return blobName;
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to upload image: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<BlobItemInfo>> GetAllImagesAsync()
    {
        var blobs = new List<BlobItemInfo>();
        
        try
        {
            await foreach (var blobItem in _imageContainerClient.GetBlobsAsync(new GetBlobsOptions { Traits = BlobTraits.Metadata }))
            {
                var blobClient = _imageContainerClient.GetBlobClient(blobItem.Name);
                
                var blobInfo = new BlobItemInfo
                {
                    BlobName = blobItem.Name,
                    Size = blobItem.Properties.ContentLength ?? 0,
                    ContentType = blobItem.Properties.ContentType ?? string.Empty,
                    Url = blobClient.Uri.ToString()
                };

                // Extract metadata
                if (blobItem.Metadata != null)
                {
                    if (blobItem.Metadata.TryGetValue(MetadataOriginalFileName, out var originalFileName))
                    {
                        blobInfo.OriginalFileName = originalFileName;
                    }
                    
                    if (blobItem.Metadata.TryGetValue(MetadataUploadTimestamp, out var timestampStr) &&
                        DateTimeOffset.TryParse(timestampStr, out var timestamp))
                    {
                        blobInfo.UploadTimestamp = timestamp;
                    }
                }

                blobs.Add(blobInfo);
            }
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve images: {ex.Message}", ex);
        }
        
        return blobs;
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadImageAsync(string blobName)
    {
        try
        {
            var blobClient = _imageContainerClient.GetBlobClient(blobName);
            var response = await blobClient.DownloadStreamingAsync();
            return response.Value.Content;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new InvalidOperationException("Image not found.", ex);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to download image: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task DeleteImageAsync(string blobName)
    {
        try
        {
            var blobClient = _imageContainerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to delete image: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public Task<string> GetImageUrlAsync(string blobName)
    {
        var blobClient = _imageContainerClient.GetBlobClient(blobName);
        return Task.FromResult(blobClient.Uri.ToString());
    }

    #endregion

    #region Multimedia Operations

    /// <inheritdoc />
    public async Task<string> UploadMultimediaAsync(Stream fileStream, string fileName, string contentType)
    {
        // Validate file type
        var extension = Path.GetExtension(fileName);
        if (!IsValidMultimediaType(extension))
        {
            throw new InvalidOperationException($"Invalid multimedia file type. Allowed types: {string.Join(", ", AllowedMultimediaExtensions)}");
        }

        // Validate file size
        if (fileStream.Length > MaxMultimediaSizeBytes)
        {
            throw new InvalidOperationException($"Multimedia file size exceeds the maximum allowed size of {MaxMultimediaSizeBytes / (1024 * 1024)} MB.");
        }

        // Generate unique blob name
        var blobName = GenerateUniqueBlobName(fileName);
        
        try
        {
            var blobClient = _multimediaContainerClient.GetBlobClient(blobName);
            
            // Prepare metadata
            var metadata = new Dictionary<string, string>
            {
                { MetadataOriginalFileName, fileName },
                { MetadataUploadTimestamp, DateTimeOffset.UtcNow.ToString("O") },
                { MetadataContentType, contentType }
            };

            // Upload with headers and metadata
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                },
                Metadata = metadata
            };

            await blobClient.UploadAsync(fileStream, uploadOptions);
            
            return blobName;
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to upload multimedia file: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<BlobItemInfo>> GetAllMultimediaAsync()
    {
        var blobs = new List<BlobItemInfo>();
        
        try
        {
            await foreach (var blobItem in _multimediaContainerClient.GetBlobsAsync(new GetBlobsOptions { Traits = BlobTraits.Metadata }))
            {
                var blobClient = _multimediaContainerClient.GetBlobClient(blobItem.Name);
                
                var blobInfo = new BlobItemInfo
                {
                    BlobName = blobItem.Name,
                    Size = blobItem.Properties.ContentLength ?? 0,
                    ContentType = blobItem.Properties.ContentType ?? string.Empty,
                    Url = blobClient.Uri.ToString()
                };

                // Extract metadata
                if (blobItem.Metadata != null)
                {
                    if (blobItem.Metadata.TryGetValue(MetadataOriginalFileName, out var originalFileName))
                    {
                        blobInfo.OriginalFileName = originalFileName;
                    }
                    
                    if (blobItem.Metadata.TryGetValue(MetadataUploadTimestamp, out var timestampStr) &&
                        DateTimeOffset.TryParse(timestampStr, out var timestamp))
                    {
                        blobInfo.UploadTimestamp = timestamp;
                    }
                }

                blobs.Add(blobInfo);
            }
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve multimedia files: {ex.Message}", ex);
        }
        
        return blobs;
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadMultimediaAsync(string blobName)
    {
        try
        {
            var blobClient = _multimediaContainerClient.GetBlobClient(blobName);
            var response = await blobClient.DownloadStreamingAsync();
            return response.Value.Content;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new InvalidOperationException("Multimedia file not found.", ex);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to download multimedia file: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task DeleteMultimediaAsync(string blobName)
    {
        try
        {
            var blobClient = _multimediaContainerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to delete multimedia file: {ex.Message}", ex);
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Generates a unique blob name using a GUID and the original file extension.
    /// </summary>
    /// <param name="originalFileName">The original filename to extract the extension from.</param>
    /// <returns>A unique blob name.</returns>
    private static string GenerateUniqueBlobName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        return $"{Guid.NewGuid()}{extension}";
    }

    /// <summary>
    /// Validates if the file extension is an allowed image type.
    /// </summary>
    /// <param name="extension">The file extension to validate.</param>
    /// <returns>True if the extension is allowed; otherwise, false.</returns>
    public static bool IsValidImageType(string extension)
    {
        return AllowedImageExtensions.Contains(extension);
    }

    /// <summary>
    /// Validates if the file extension is an allowed multimedia type.
    /// </summary>
    /// <param name="extension">The file extension to validate.</param>
    /// <returns>True if the extension is allowed; otherwise, false.</returns>
    public static bool IsValidMultimediaType(string extension)
    {
        return AllowedMultimediaExtensions.Contains(extension);
    }

    #endregion
}
