namespace ABCRetail.Models;

/// <summary>
/// Represents an inventory management message to be sent to Azure Queue Storage.
/// Contains inventory update details and provides formatted message generation.
/// </summary>
public class InventoryMessage
{
    /// <summary>
    /// Identifier of the product for the inventory action.
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Type of inventory action: "Restock", "Deduct", or "Alert".
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// Quantity of units affected by the inventory action.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Reason or description for the inventory action.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Generates a formatted message string for the Azure Queue.
    /// Format: "{ActionType} inventory for {ProductId}: {Quantity} units - {Reason}"
    /// </summary>
    /// <returns>Formatted queue message string.</returns>
    public string ToQueueMessage() =>
        $"{ActionType} inventory for {ProductId}: {Quantity} units - {Reason}";
}
