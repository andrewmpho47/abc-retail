using Azure;
using Azure.Data.Tables;

namespace ABCRetail.Models;

/// <summary>
/// Represents a product entity stored in Azure Table Storage.
/// Uses PartitionKey for category grouping and RowKey as the unique product identifier.
/// </summary>
public class Product : ITableEntity
{
    /// <summary>
    /// Partition key representing the product category.
    /// </summary>
    public string PartitionKey { get; set; } = string.Empty;

    /// <summary>
    /// Row key serving as the unique product identifier within the category.
    /// </summary>
    public string RowKey { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of the last modification, managed by Azure Table Storage.
    /// </summary>
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>
    /// Entity tag for optimistic concurrency control.
    /// </summary>
    public ETag ETag { get; set; }

    // Business Properties

    /// <summary>
    /// Name of the product.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the product.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Price of the product in the default currency.
    /// </summary>
    public double Price { get; set; }

    /// <summary>
    /// Current stock quantity available.
    /// </summary>
    public int StockQuantity { get; set; }
}
