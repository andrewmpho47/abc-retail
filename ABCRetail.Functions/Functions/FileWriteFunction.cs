using ABCRetail.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace ABCRetail.Functions.Functions;

public sealed class FileWriteFunction
{
    private readonly StorageClientFactory _storageClientFactory;
    private readonly ILogger<FileWriteFunction> _logger;

    public FileWriteFunction(
        StorageClientFactory storageClientFactory,
        ILogger<FileWriteFunction> logger)
    {
        _storageClientFactory = storageClientFactory;
        _logger = logger;
    }

    [Function("SendFileToAzureFiles")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "files/logs")] HttpRequest request)
    {
        var payload = await JsonSerializer.DeserializeAsync<FileWriteRequest>(
            request.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new FileWriteRequest();

        var fileName = string.IsNullOrWhiteSpace(payload.FileName)
            ? $"project2-function-log-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.txt"
            : payload.FileName;

        var content = string.IsNullOrWhiteSpace(payload.Content)
            ? $"ABC Retail Project 2 Azure Files evidence generated at {DateTimeOffset.UtcNow:O}{Environment.NewLine}"
            : payload.Content;

        var contentBytes = Encoding.UTF8.GetBytes(content);
        var fileClient = _storageClientFactory.LogFileRootDirectory.GetFileClient(fileName);
        await fileClient.CreateAsync(contentBytes.Length);
        await using var stream = new MemoryStream(contentBytes);
        await fileClient.UploadAsync(stream);
        _logger.LogInformation("Created file {FileName} in Azure Files.", fileName);

        return new OkObjectResult(new
        {
            message = "File sent to Azure Files.",
            fileName,
            size = content.Length
        });
    }
}
