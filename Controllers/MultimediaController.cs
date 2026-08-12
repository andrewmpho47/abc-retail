using Microsoft.AspNetCore.Mvc;
using ABCRetail.Services.Interfaces;

namespace ABCRetail.Controllers;

/// <summary>
/// Controller for managing multimedia files stored in Azure Blob Storage.
/// </summary>
public class MultimediaController : Controller
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly IStorageErrorLogger _errorLogger;
    private const string ServiceName = "BlobStorageService";

    public MultimediaController(
        IBlobStorageService blobStorageService,
        IStorageErrorLogger errorLogger)
    {
        _blobStorageService = blobStorageService;
        _errorLogger = errorLogger;
    }

    /// <summary>
    /// Displays the multimedia files list.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var files = await _blobStorageService.GetAllMultimediaAsync();
            return View(files);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("GetAllMultimedia", ServiceName, ex);
            TempData["Error"] = $"Failed to load multimedia files: {ex.Message}";
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
    /// Handles multimedia file upload.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select a file to upload.";
            return View();
        }

        // Validate file type
        var allowedExtensions = new[] { ".pdf", ".mp4", ".docx", ".xlsx" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            TempData["Error"] = $"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}";
            return View();
        }

        // Validate file size (100MB max)
        if (file.Length > 100 * 1024 * 1024)
        {
            TempData["Error"] = "File size exceeds the maximum allowed size of 100 MB.";
            return View();
        }

        try
        {
            using var stream = file.OpenReadStream();
            await _blobStorageService.UploadMultimediaAsync(stream, file.FileName, file.ContentType);
            
            TempData["Success"] = $"File '{file.FileName}' uploaded successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("UploadMultimedia", ServiceName, ex, $"FileName: {file.FileName}");
            TempData["Error"] = $"Failed to upload file: {ex.Message}";
            return View();
        }
    }

    /// <summary>
    /// Downloads a multimedia file.
    /// </summary>
    public async Task<IActionResult> Download(string blobName)
    {
        if (string.IsNullOrEmpty(blobName))
        {
            TempData["Error"] = "File name is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var stream = await _blobStorageService.DownloadMultimediaAsync(blobName);
            var contentType = GetContentType(blobName);
            return File(stream, contentType, blobName);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("DownloadMultimedia", ServiceName, ex, $"BlobName: {blobName}");
            TempData["Error"] = $"Failed to download file: {ex.Message}";
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
            TempData["Error"] = "File name is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var files = await _blobStorageService.GetAllMultimediaAsync();
            var file = files.FirstOrDefault(f => f.BlobName == blobName);
            if (file == null)
            {
                TempData["Error"] = "File not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(file);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("GetMultimediaForDelete", ServiceName, ex, $"BlobName: {blobName}");
            TempData["Error"] = $"Failed to load file: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Handles multimedia file deletion.
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string blobName)
    {
        try
        {
            await _blobStorageService.DeleteMultimediaAsync(blobName);
            TempData["Success"] = "File deleted successfully.";
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("DeleteMultimedia", ServiceName, ex, $"BlobName: {blobName}");
            TempData["Error"] = $"Failed to delete file: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".mp4" => "video/mp4",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
    }
}
