using Microsoft.AspNetCore.Mvc;
using ABCRetail.Services.Interfaces;

namespace ABCRetail.Controllers;

/// <summary>
/// Controller for managing images stored in Azure Blob Storage.
/// </summary>
public class ImageController : Controller
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly IQueueStorageService _queueStorageService;
    private readonly IStorageErrorLogger _errorLogger;
    private const string BlobServiceName = "BlobStorageService";
    private const string QueueServiceName = "QueueStorageService";

    public ImageController(
        IBlobStorageService blobStorageService,
        IQueueStorageService queueStorageService,
        IStorageErrorLogger errorLogger)
    {
        _blobStorageService = blobStorageService;
        _queueStorageService = queueStorageService;
        _errorLogger = errorLogger;
    }

    /// <summary>
    /// Displays the image gallery with all images from Blob Storage.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var images = await _blobStorageService.GetAllImagesAsync();
            var notifications = await _queueStorageService.PeekImageNotificationsAsync(10);
            ViewBag.Notifications = notifications;
            return View(images);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("GetAllImages", BlobServiceName, ex);
            TempData["Error"] = $"Failed to load images: {ex.Message}";
            return View(Enumerable.Empty<ABCRetail.Models.BlobItemInfo>());
        }
    }

    /// <summary>
    /// Displays the upload form.
    /// </summary>
    public IActionResult Upload()
    {
        return View();
    }

    /// <summary>
    /// Handles image file upload.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select an image file to upload.";
            return View();
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            TempData["Error"] = $"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}";
            return View();
        }

        // Validate file size (10MB max)
        if (file.Length > 10 * 1024 * 1024)
        {
            TempData["Error"] = "File size exceeds the maximum allowed size of 10 MB.";
            return View();
        }

        try
        {
            using var stream = file.OpenReadStream();
            var blobName = await _blobStorageService.UploadImageAsync(stream, file.FileName, file.ContentType);
            
            // Send notification to queue
            await _queueStorageService.SendImageUploadNotificationAsync(file.FileName);
            
            TempData["Success"] = $"Image '{file.FileName}' uploaded successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("UploadImage", BlobServiceName, ex, $"FileName: {file.FileName}");
            TempData["Error"] = $"Failed to upload image: {ex.Message}";
            return View();
        }
    }

    /// <summary>
    /// Downloads an image file.
    /// </summary>
    public async Task<IActionResult> Download(string blobName)
    {
        if (string.IsNullOrEmpty(blobName))
        {
            TempData["Error"] = "Image name is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var stream = await _blobStorageService.DownloadImageAsync(blobName);
            var contentType = GetContentType(blobName);
            return File(stream, contentType, blobName);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("DownloadImage", BlobServiceName, ex, $"BlobName: {blobName}");
            TempData["Error"] = $"Failed to download image: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Displays the delete confirmation page.
    /// </summary>
    public async Task<IActionResult> Delete(string blobName)
    {
        if (string.IsNullOrEmpty(blobName))
        {
            TempData["Error"] = "Image name is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var images = await _blobStorageService.GetAllImagesAsync();
            var image = images.FirstOrDefault(i => i.BlobName == blobName);
            if (image == null)
            {
                TempData["Error"] = "Image not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(image);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("GetImageForDelete", BlobServiceName, ex, $"BlobName: {blobName}");
            TempData["Error"] = $"Failed to load image: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Handles image deletion.
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string blobName)
    {
        try
        {
            await _blobStorageService.DeleteImageAsync(blobName);
            TempData["Success"] = "Image deleted successfully.";
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("DeleteImage", BlobServiceName, ex, $"BlobName: {blobName}");
            TempData["Error"] = $"Failed to delete image: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
