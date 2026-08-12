namespace ABCRetail.Models;

/// <summary>
/// Represents an order processing message to be sent to Azure Queue Storage.
/// Contains order details and provides formatted message generation.
/// </summary>
public class OrderMessage
{
    /// <summary>
    /// Unique identifier for the order.
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the customer placing the order.
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the product being ordered.
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Quantity of the product being ordered.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Current status of the order (e.g., "Pending", "Processing", "Completed").
    /// </summary>
    public string OrderStatus { get; set; } = string.Empty;

    /// <summary>
    /// Generates a formatted message string for the Azure Queue.
    /// Format: "Processing order {OrderId} for customer {CustomerId}: {Quantity} x {ProductId}"
    /// </summary>
    /// <returns>Formatted queue message string.</returns>
    public string ToQueueMessage() =>
        $"Processing order {OrderId} for customer {CustomerId}: {Quantity} x {ProductId}";
}
