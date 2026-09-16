using ABCRetail.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ABCRetail.Functions.Functions;

public sealed class QueueTransactionFunction
{
    private readonly StorageClientFactory _storageClientFactory;
    private readonly ILogger<QueueTransactionFunction> _logger;

    public QueueTransactionFunction(
        StorageClientFactory storageClientFactory,
        ILogger<QueueTransactionFunction> logger)
    {
        _storageClientFactory = storageClientFactory;
        _logger = logger;
    }

    [Function("ReadWriteOrderQueue")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", "delete", Route = "queues/orders")] HttpRequest request)
    {
        if (HttpMethods.IsPost(request.Method))
        {
            var payload = await JsonSerializer.DeserializeAsync<QueueTransactionRequest>(
                request.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new QueueTransactionRequest();

            var orderId = string.IsNullOrWhiteSpace(payload.OrderId) ? $"ORD-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}" : payload.OrderId;
            var customerId = string.IsNullOrWhiteSpace(payload.CustomerId) ? "Customer-Project2" : payload.CustomerId;
            var productId = string.IsNullOrWhiteSpace(payload.ProductId) ? "Product-Project2" : payload.ProductId;
            var quantity = payload.Quantity <= 0 ? 1 : payload.Quantity;
            var orderStatus = string.IsNullOrWhiteSpace(payload.OrderStatus) ? "Submitted" : payload.OrderStatus;
            var queueMessage = $"OrderId:{orderId}|CustomerId:{customerId}|ProductId:{productId}|Quantity:{quantity}|Status:{orderStatus}|Timestamp:{DateTimeOffset.UtcNow:O}";

            await _storageClientFactory.OrderQueue.SendMessageAsync(queueMessage);
            _logger.LogInformation("Wrote order {OrderId} to Azure Queue Storage.", orderId);

            return new OkObjectResult(new
            {
                message = "Order transaction written to Azure Queue Storage.",
                queueMessage
            });
        }

        if (HttpMethods.IsDelete(request.Method))
        {
            var response = await _storageClientFactory.OrderQueue.ReceiveMessageAsync();
            var processed = response.Value;
            if (processed is not null)
            {
                await _storageClientFactory.OrderQueue.DeleteMessageAsync(processed.MessageId, processed.PopReceipt);
            }

            return new OkObjectResult(new
            {
                message = processed is null ? "No queue message available to process." : "Order transaction read and removed from Azure Queue Storage.",
                processed = processed is null ? null : new
                {
                    processed.MessageId,
                    Content = processed.MessageText,
                    processed.InsertedOn,
                    processed.ExpiresOn
                }
            });
        }

        var messages = await _storageClientFactory.OrderQueue.PeekMessagesAsync(32);
        var properties = await _storageClientFactory.OrderQueue.GetPropertiesAsync();

        return new OkObjectResult(new
        {
            message = "Order transactions read from Azure Queue Storage.",
            approximateMessageCount = properties.Value.ApproximateMessagesCount,
            messages = messages.Value.Select(message => new
            {
                message.MessageId,
                Content = message.MessageText,
                message.InsertedOn,
                message.ExpiresOn
            })
        });
    }
}
