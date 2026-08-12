using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using ABCRetail.Models;
using ABCRetail.Services.Interfaces;

namespace ABCRetail.Services;

/// <summary>
/// Implements Azure Queue Storage operations for order processing, inventory management,
/// and image upload notifications.
/// </summary>
public class QueueStorageService : IQueueStorageService
{
    private readonly QueueClient _orderQueueClient;
    private readonly QueueClient _inventoryQueueClient;
    private readonly QueueClient _imageNotificationQueueClient;

    /// <summary>
    /// Initializes a new instance of the QueueStorageService.
    /// </summary>
    /// <param name="settings">The Azure Storage settings containing connection strings and queue names.</param>
    public QueueStorageService(AzureStorageSettings settings)
    {
        var connectionString = settings.GetQueueConnectionString();

        // Create queue clients and ensure queues exist
        _orderQueueClient = new QueueClient(connectionString, settings.OrderQueueName);
        _orderQueueClient.CreateIfNotExists();

        _inventoryQueueClient = new QueueClient(connectionString, settings.InventoryQueueName);
        _inventoryQueueClient.CreateIfNotExists();

        _imageNotificationQueueClient = new QueueClient(connectionString, settings.ImageNotificationQueueName);
        _imageNotificationQueueClient.CreateIfNotExists();
    }

    #region Order Queue Operations

    /// <inheritdoc />
    public async Task SendOrderMessageAsync(OrderMessage order)
    {
        try
        {
            var messageText = order.ToQueueMessage();
            await _orderQueueClient.SendMessageAsync(messageText);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to send order message: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QueueMessageInfo>> PeekOrderMessagesAsync(int maxMessages = 32)
    {
        try
        {
            var peekedMessages = await _orderQueueClient.PeekMessagesAsync(maxMessages);
            return MapToQueueMessageInfo(peekedMessages.Value);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to peek order messages: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<QueueMessageInfo?> DequeueOrderMessageAsync()
    {
        try
        {
            var response = await _orderQueueClient.ReceiveMessageAsync();
            
            if (response.Value == null)
            {
                return null;
            }

            var message = response.Value;
            
            // Delete the message from the queue after receiving it
            await _orderQueueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);

            return new QueueMessageInfo
            {
                MessageId = message.MessageId,
                Content = message.MessageText,
                InsertedOn = message.InsertedOn,
                ExpiresOn = message.ExpiresOn,
                PopReceipt = message.PopReceipt
            };
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to dequeue order message: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<int> GetOrderQueueCountAsync()
    {
        try
        {
            var properties = await _orderQueueClient.GetPropertiesAsync();
            return properties.Value.ApproximateMessagesCount;
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to get order queue count: {ex.Message}", ex);
        }
    }

    #endregion

    #region Inventory Queue Operations

    /// <inheritdoc />
    public async Task SendInventoryMessageAsync(InventoryMessage inventory)
    {
        try
        {
            var messageText = inventory.ToQueueMessage();
            await _inventoryQueueClient.SendMessageAsync(messageText);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to send inventory message: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QueueMessageInfo>> PeekInventoryMessagesAsync(int maxMessages = 32)
    {
        try
        {
            var peekedMessages = await _inventoryQueueClient.PeekMessagesAsync(maxMessages);
            return MapToQueueMessageInfo(peekedMessages.Value);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to peek inventory messages: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<QueueMessageInfo?> DequeueInventoryMessageAsync()
    {
        try
        {
            var response = await _inventoryQueueClient.ReceiveMessageAsync();
            
            if (response.Value == null)
            {
                return null;
            }

            var message = response.Value;
            
            // Delete the message from the queue after receiving it
            await _inventoryQueueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);

            return new QueueMessageInfo
            {
                MessageId = message.MessageId,
                Content = message.MessageText,
                InsertedOn = message.InsertedOn,
                ExpiresOn = message.ExpiresOn,
                PopReceipt = message.PopReceipt
            };
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to dequeue inventory message: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<int> GetInventoryQueueCountAsync()
    {
        try
        {
            var properties = await _inventoryQueueClient.GetPropertiesAsync();
            return properties.Value.ApproximateMessagesCount;
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to get inventory queue count: {ex.Message}", ex);
        }
    }

    #endregion

    #region Image Upload Notification Queue Operations

    /// <inheritdoc />
    public async Task SendImageUploadNotificationAsync(string imageName)
    {
        try
        {
            var messageText = $"Uploading an image {imageName} to blob storage";
            await _imageNotificationQueueClient.SendMessageAsync(messageText);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to send image upload notification: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QueueMessageInfo>> PeekImageNotificationsAsync(int maxMessages = 32)
    {
        try
        {
            var peekedMessages = await _imageNotificationQueueClient.PeekMessagesAsync(maxMessages);
            return MapToQueueMessageInfo(peekedMessages.Value);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to peek image notification messages: {ex.Message}", ex);
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Maps an array of PeekedMessage to a collection of QueueMessageInfo.
    /// </summary>
    /// <param name="peekedMessages">The peeked messages from the queue.</param>
    /// <returns>A collection of QueueMessageInfo objects.</returns>
    private static IEnumerable<QueueMessageInfo> MapToQueueMessageInfo(PeekedMessage[] peekedMessages)
    {
        return peekedMessages.Select(m => new QueueMessageInfo
        {
            MessageId = m.MessageId,
            Content = m.MessageText,
            InsertedOn = m.InsertedOn,
            ExpiresOn = m.ExpiresOn,
            PopReceipt = string.Empty // PeekedMessage doesn't have PopReceipt
        });
    }

    #endregion
}
