using Azure;
using Azure.Data.Tables;

namespace ABCRetail.Models;

/// <summary>
/// Represents a customer profile entity stored in Azure Table Storage.
/// Uses PartitionKey for logical grouping and RowKey as the unique customer identifier.
/// </summary>
public class CustomerProfile : ITableEntity
{
    /// <summary>
    /// Partition key for logical grouping of customer records.
    /// </summary>
    public string PartitionKey { get; set; } = string.Empty;

    /// <summary>
    /// Row key serving as the unique customer identifier within the partition.
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
    /// Customer's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Customer's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Customer's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Customer's phone number.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Customer's physical address.
    /// </summary>
    public string Address { get; set; } = string.Empty;
}
