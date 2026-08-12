using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using ABCRetail.Models;
using ABCRetail.Services.Interfaces;
using System.Text;

namespace ABCRetail.Services;

/// <summary>
/// Implements Azure Files operations for application log files.
/// Uses Azure.Storage.Files.Shares SDK for file share interactions.
/// Falls back to local file storage when using development emulator (Azurite doesn't support Azure Files).
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly ShareClient? _shareClient;
    private readonly ShareDirectoryClient? _rootDirectory;
    private readonly string? _localLogDirectory;
    private readonly bool _useLocalStorage;

    /// <summary>
    /// Initializes a new instance of the FileStorageService.
    /// </summary>
    /// <param name="settings">The Azure Storage settings containing connection strings and share names.</param>
    public FileStorageService(AzureStorageSettings settings)
    {
        var connectionString = settings.GetFileConnectionString();
        
        // Check if using development storage (Azurite) - Azure Files doesn't support emulator
        _useLocalStorage = connectionString.Contains("UseDevelopmentStorage=true", StringComparison.OrdinalIgnoreCase)
                        || connectionString.Contains("127.0.0.1:10000", StringComparison.OrdinalIgnoreCase)
                        || connectionString.Contains("devstoreaccount1", StringComparison.OrdinalIgnoreCase);
        
        if (_useLocalStorage)
        {
            // Use local file system for development
            _localLogDirectory = Path.Combine(Directory.GetCurrentDirectory(), "LocalStorage", "logs");
            Directory.CreateDirectory(_localLogDirectory);
        }
        else
        {
            // Use Azure Files for production
            _shareClient = new ShareClient(connectionString, settings.LogFileShareName);
            _shareClient.CreateIfNotExists();
            _rootDirectory = _shareClient.GetRootDirectoryClient();
        }
    }

    /// <inheritdoc />
    public async Task CreateLogFileAsync(string fileName, string content)
    {
        if (_useLocalStorage)
        {
            var filePath = Path.Combine(_localLogDirectory!, fileName);
            await File.WriteAllTextAsync(filePath, content);
            return;
        }
        
        try
        {
            var fileClient = _rootDirectory!.GetFileClient(fileName);
            
            // Convert content to bytes
            var contentBytes = Encoding.UTF8.GetBytes(content);
            
            // Create the file with the specified size
            await fileClient.CreateAsync(contentBytes.Length);
            
            // Upload the content if there is any
            if (contentBytes.Length > 0)
            {
                using var stream = new MemoryStream(contentBytes);
                await fileClient.UploadAsync(stream);
            }
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to create log file: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LogFileInfo>> GetAllLogFilesAsync()
    {
        if (_useLocalStorage)
        {
            var files = Directory.GetFiles(_localLogDirectory!);
            return files.Select(f => new FileInfo(f)).Select(fi => new LogFileInfo
            {
                FileName = fi.Name,
                Size = fi.Length,
                LastModified = fi.LastWriteTimeUtc
            });
        }
        
        var logFiles = new List<LogFileInfo>();
        
        try
        {
            await foreach (var item in _rootDirectory!.GetFilesAndDirectoriesAsync())
            {
                // Only include files, not directories
                if (!item.IsDirectory)
                {
                    var fileClient = _rootDirectory.GetFileClient(item.Name);
                    var properties = await fileClient.GetPropertiesAsync();
                    
                    logFiles.Add(new LogFileInfo
                    {
                        FileName = item.Name,
                        Size = properties.Value.ContentLength,
                        LastModified = properties.Value.LastModified
                    });
                }
            }
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve log files: {ex.Message}", ex);
        }
        
        return logFiles;
    }

    /// <inheritdoc />
    public async Task<string> GetLogFileContentAsync(string fileName)
    {
        if (_useLocalStorage)
        {
            var filePath = Path.Combine(_localLogDirectory!, fileName);
            if (!File.Exists(filePath))
                throw new InvalidOperationException("Log file not found.");
            return await File.ReadAllTextAsync(filePath);
        }
        
        try
        {
            var fileClient = _rootDirectory!.GetFileClient(fileName);
            var response = await fileClient.DownloadAsync();
            
            using var reader = new StreamReader(response.Value.Content, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new InvalidOperationException("Log file not found.", ex);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to read log file: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadLogFileAsync(string fileName)
    {
        if (_useLocalStorage)
        {
            var filePath = Path.Combine(_localLogDirectory!, fileName);
            if (!File.Exists(filePath))
                throw new InvalidOperationException("Log file not found.");
            var bytes = await File.ReadAllBytesAsync(filePath);
            return new MemoryStream(bytes);
        }
        
        try
        {
            var fileClient = _rootDirectory!.GetFileClient(fileName);
            var response = await fileClient.DownloadAsync();
            
            // Copy to a MemoryStream that can be used after the response is disposed
            var memoryStream = new MemoryStream();
            await response.Value.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            return memoryStream;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new InvalidOperationException("Log file not found.", ex);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to download log file: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task DeleteLogFileAsync(string fileName)
    {
        if (_useLocalStorage)
        {
            var filePath = Path.Combine(_localLogDirectory!, fileName);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return;
        }
        
        try
        {
            var fileClient = _rootDirectory!.GetFileClient(fileName);
            await fileClient.DeleteIfExistsAsync();
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to delete log file: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task AppendToLogFileAsync(string fileName, string content)
    {
        if (_useLocalStorage)
        {
            var filePath = Path.Combine(_localLogDirectory!, fileName);
            await File.AppendAllTextAsync(filePath, content);
            return;
        }
        
        try
        {
            var fileClient = _rootDirectory!.GetFileClient(fileName);
            
            // Check if file exists
            bool fileExists = await fileClient.ExistsAsync();
            
            string existingContent = string.Empty;
            
            if (fileExists)
            {
                // Read existing content
                var downloadResponse = await fileClient.DownloadAsync();
                using var reader = new StreamReader(downloadResponse.Value.Content, Encoding.UTF8);
                existingContent = await reader.ReadToEndAsync();
            }
            
            // Combine existing content with new content
            var newContent = existingContent + content;
            var contentBytes = Encoding.UTF8.GetBytes(newContent);
            
            // Resize the file to accommodate new content
            await fileClient.CreateAsync(contentBytes.Length);
            
            // Upload the combined content
            if (contentBytes.Length > 0)
            {
                using var stream = new MemoryStream(contentBytes);
                await fileClient.UploadAsync(stream);
            }
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to append to log file: {ex.Message}", ex);
        }
    }
}
