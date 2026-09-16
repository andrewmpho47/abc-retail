using ABCRetail.Functions.Models;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ABCRetail.Functions.Functions;

public sealed class TableInfoFunction
{
    private readonly StorageClientFactory _storageClientFactory;
    private readonly ILogger<TableInfoFunction> _logger;

    public TableInfoFunction(
        StorageClientFactory storageClientFactory,
        ILogger<TableInfoFunction> logger)
    {
        _storageClientFactory = storageClientFactory;
        _logger = logger;
    }

    [Function("StoreCustomerInTable")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "tables/customers")] HttpRequest request)
    {
        var payload = await JsonSerializer.DeserializeAsync<CustomerTableRequest>(
            request.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new CustomerTableRequest();

        var partitionKey = string.IsNullOrWhiteSpace(payload.PartitionKey) ? "Customer" : payload.PartitionKey;
        var rowKey = string.IsNullOrWhiteSpace(payload.RowKey) ? Guid.NewGuid().ToString("N") : payload.RowKey;
        var customer = new TableEntity(partitionKey, rowKey)
        {
            ["FirstName"] = string.IsNullOrWhiteSpace(payload.FirstName) ? "Project" : payload.FirstName,
            ["LastName"] = string.IsNullOrWhiteSpace(payload.LastName) ? "Two" : payload.LastName,
            ["Email"] = string.IsNullOrWhiteSpace(payload.Email) ? "project2@example.com" : payload.Email,
            ["PhoneNumber"] = payload.PhoneNumber ?? string.Empty,
            ["Address"] = payload.Address ?? "ABC Retail Azure Function",
            ["CreatedBy"] = "StoreCustomerInTable Azure Function",
            ["CreatedUtc"] = DateTimeOffset.UtcNow
        };

        await _storageClientFactory.CustomerTable.AddEntityAsync(customer);
        _logger.LogInformation("Stored customer {PartitionKey}/{RowKey} in Azure Table Storage.", partitionKey, rowKey);

        return new OkObjectResult(new
        {
            message = "Customer stored in Azure Table Storage.",
            partitionKey,
            rowKey,
            firstName = customer["FirstName"],
            lastName = customer["LastName"],
            email = customer["Email"]
        });
    }
}
