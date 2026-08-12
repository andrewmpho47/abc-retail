using ABCRetail.Models;

namespace ABCRetail.Services.Interfaces;

/// <summary>
/// Interface for Azure Queue Storage operations.
/// Provides methods for order processing, inventory management, and image upload notification queues.
/// </summary>
public interface IQueueStorageService
{
    #region Order Queue Operations

    /// <summary>
    /// Sends an order message to the order processing queue.
    /// </summary>
    /// <param name="order">The order message to send.</param>
    Task SendOrderMessageAsync(OrderMessage order);

    /// <summary>
    /// Peeks at messages in the order processing queue without removing them.
    /// </summary>
    /// <param name="maxMessages">Maximum number of messages to peek (1-32).</param>
    /// <returns>A collection of queue message information.</returns>
    Task<IEnumerable<QueueMessageInfo>> PeekOrderMessagesAsync(int maxMessages = 32);

    /// <summary>
    /// Dequeues (removes and returns) the next message from the order processing queue.
    /// </summary>
    /// <returns>The dequeued message information, or null if the queue is empty.</returns>
    Task<QueueMessageInfo?> DequeueOrderMessageAsync();

    /// <summary>
    /// Gets the approximate count of messages in the order processing queue.
    /// </summary>
    /// <returns>The approximate message count.</returns>
    Task<int> GetOrderQueueCountAsync();

    #endregion

    #region Inventory Queue Operations

    /// <summary>
    /// Sends an inventory message to the inventory management queue.
    /// </summary>
    /// <param name="inventory">The inventory message to send.</param>
    Task SendInventoryMessageAsync(InventoryMessage inventory);

    /// <summary>
    /// Peeks at messages in the inventory management queue without removing them.
    /// </summary>
    /// <param name="maxMessages">Maximum number of messages to peek (1-32).</param>
    /// <returns>A collection of queue message information.</returns>
    Task<IEnumerable<QueueMessageInfo>> PeekInventoryMessagesAsync(int maxMessages = 32);

    /// <summary>
    /// Dequeues (removes and returns) the next message from the inventory management queue.
    /// </summary>
    /// <returns>The dequeued message information, or null if the queue is empty.</returns>
    Task<QueueMessageInfo?> DequeueInventoryMessageAsync();

    /// <summary>
    /// Gets the approximate count of messages in the inventory management queue.
    /// </summary>
    /// <returns>The approximate message count.</returns>
    Task<int> GetInventoryQueueCountAsync();

    #endregion

    #region Image Upload Notification Queue Operations

    /// <summary>
    /// Sends an image upload notification message to the notification queue.
    /// </summary>
    /// <param name="imageName">The name of the uploaded image.</param>
    Task SendImageUploadNotificationAsync(string imageName);

    /// <summary>
    /// Peeks at messages in the image notification queue without removing them.
    /// </summary>
    /// <param name="maxMessages">Maximum number of messages to peek (1-32).</param>
    /// <returns>A collection of queue message information.</returns>
    Task<IEnumerable<QueueMessageInfo>> PeekImageNotificationsAsync(int maxMessages = 32);

    #endregion
}
