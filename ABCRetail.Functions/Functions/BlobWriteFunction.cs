using ABCRetail.Functions.Models;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ABCRetail.Functions.Functions;

public sealed class BlobWriteFunction
{
    private static readonly byte[] SamplePng =
    [
        137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82,
        0, 0, 0, 1, 0, 0, 0, 1, 8, 6, 0, 0, 0, 31, 21, 196, 137,
        0, 0, 0, 13, 73, 68, 65, 84, 120, 156, 99, 248, 15, 4, 0,
        9, 251, 3, 253, 167, 89, 229, 27, 0, 0, 0, 0, 73, 69, 78, 68,
        174, 66, 96, 130
    ];

    private readonly StorageClientFactory _storageClientFactory;
    private readonly ILogger<BlobWriteFunction> _logger;

    public BlobWriteFunction(
        StorageClientFactory storageClientFactory,
        ILogger<BlobWriteFunction> logger)
    {
        _storageClientFactory = storageClientFactory;
        _logger = logger;
    }

    [Function("WriteImageToBlobStorage")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "blobs/images")] HttpRequest request)
    {
        string fileName;
        string contentType;
        Stream contentStream;

        if (request.HasFormContentType && request.Form.Files.Count > 0)
        {
            var file = request.Form.Files[0];
            fileName = file.FileName;
            contentType = file.ContentType;
            contentStream = file.OpenReadStream();
        }
        else
        {
            var payload = await JsonSerializer.DeserializeAsync<BlobWriteRequest>(
                request.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new BlobWriteRequest();

            fileName = string.IsNullOrWhiteSpace(payload.FileName)
                ? $"project2-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.png"
                : payload.FileName;
            contentType = string.IsNullOrWhiteSpace(payload.ContentType) ? "image/png" : payload.ContentType;

            var bytes = string.IsNullOrWhiteSpace(payload.Base64Content)
                ? SamplePng
                : Convert.FromBase64String(payload.Base64Content);
            contentStream = new MemoryStream(bytes);
        }

        await using (contentStream)
        {
            var blobName = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
            var blobClient = _storageClientFactory.ImageContainer.GetBlobClient(blobName);
            await blobClient.UploadAsync(contentStream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                Metadata = new Dictionary<string, string>
                {
                    ["OriginalFileName"] = fileName,
                    ["UploadedBy"] = "WriteImageToBlobStorage Azure Function",
                    ["UploadTimestamp"] = DateTimeOffset.UtcNow.ToString("O")
                }
            });

            _logger.LogInformation("Uploaded {BlobName} to Azure Blob Storage.", blobName);

            return new OkObjectResult(new
            {
                message = "Image written to Azure Blob Storage.",
                blobName,
                fileName,
                contentType
            });
        }
    }
}
