using Microsoft.AspNetCore.Mvc;
using ABCRetail.Models;
using ABCRetail.Models.ViewModels;
using ABCRetail.Services.Interfaces;

namespace ABCRetail.Controllers;

/// <summary>
/// Controller for managing log files stored in Azure Files.
/// </summary>
public class LogController : Controller
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IStorageErrorLogger _errorLogger;
    private const string ServiceName = "FileStorageService";

    public LogController(
        IFileStorageService fileStorageService,
        IStorageErrorLogger errorLogger)
    {
        _fileStorageService = fileStorageService;
        _errorLogger = errorLogger;
    }

    /// <summary>
    /// Displays the list of log files.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var files = await _fileStorageService.GetAllLogFilesAsync();
            return View(files);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("GetAllLogFiles", ServiceName, ex);
            TempData["Error"] = $"Failed to load log files: {ex.Message}";
            return View(Enumerable.Empty<LogFileInfo>());
        }
    }

    /// <summary>
    /// Displays the create log file form.
    /// </summary>
    public IActionResult Create()
    {
        return View(new LogFileFormViewModel());
    }

    /// <summary>
    /// Handles creating a new log file.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LogFileFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Ensure filename has .txt or .log extension
            var fileName = model.FileName;
            if (!fileName.EndsWith(".txt") && !fileName.EndsWith(".log"))
            {
                fileName += ".txt";
            }

            await _fileStorageService.CreateLogFileAsync(fileName, model.Content);
            TempData["Success"] = $"Log file '{fileName}' created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("CreateLogFile", ServiceName, ex, $"FileName: {model.FileName}");
            TempData["Error"] = $"Failed to create log file: {ex.Message}";
            return View(model);
        }
    }

    /// <summary>
    /// Displays the content of a log file.
    /// </summary>
    public async Task<IActionResult> View(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            TempData["Error"] = "File name is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var content = await _fileStorageService.GetLogFileContentAsync(fileName);
            ViewBag.FileName = fileName;
            ViewBag.Content = content;
            return View();
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("GetLogFileContent", ServiceName, ex, $"FileName: {fileName}");
            TempData["Error"] = $"Failed to read log file: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Downloads a log file.
    /// </summary>
    public async Task<IActionResult> Download(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            TempData["Error"] = "File name is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var stream = await _fileStorageService.DownloadLogFileAsync(fileName);
            return File(stream, "text/plain", fileName);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("DownloadLogFile", ServiceName, ex, $"FileName: {fileName}");
            TempData["Error"] = $"Failed to download log file: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Displays the delete confirmation page.
    /// </summary>
    public async Task<IActionResult> Delete(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            TempData["Error"] = "File name is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var files = await _fileStorageService.GetAllLogFilesAsync();
            var file = files.FirstOrDefault(f => f.FileName == fileName);
            if (file == null)
            {
                TempData["Error"] = "File not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(file);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("GetLogFileForDelete", ServiceName, ex, $"FileName: {fileName}");
            TempData["Error"] = $"Failed to load file: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Handles log file deletion.
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string fileName)
    {
        try
        {
            await _fileStorageService.DeleteLogFileAsync(fileName);
            TempData["Success"] = "Log file deleted successfully.";
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("DeleteLogFile", ServiceName, ex, $"FileName: {fileName}");
            TempData["Error"] = $"Failed to delete log file: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
