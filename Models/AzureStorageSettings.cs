namespace ABCRetail.Models;

/// <summary>
/// Configuration settings for Azure Storage Services.
/// Supports both shared connection string and separate connection strings per service.
/// </summary>
public class AzureStorageSettings
{
    /// <summary>
    /// Shared connection string for all Azure Storage services.
    /// Individual connection strings take precedence if specified.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Connection string specifically for Azure Table Storage.
    /// Falls back to ConnectionString if not specified.
    /// </summary>
    public string TableConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Connection string specifically for Azure Blob Storage.
    /// Falls back to ConnectionString if not specified.
    /// </summary>
    public string BlobConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Connection string specifically for Azure Queue Storage.
    /// Falls back to ConnectionString if not specified.
    /// </summary>
    public string QueueConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Connection string specifically for Azure File Storage.
    /// Falls back to ConnectionString if not specified.
    /// </summary>
    public string FileConnectionString { get; set; } = string.Empty;

    // Table Names
    /// <summary>
    /// Name of the Azure Table for storing customer profiles.
    /// </summary>
    public string CustomerTableName { get; set; } = "customers";

    /// <summary>
    /// Name of the Azure Table for storing product information.
    /// </summary>
    public string ProductTableName { get; set; } = "products";

    // Blob Container Names
    /// <summary>
    /// Name of the Azure Blob container for storing product images.
    /// </summary>
    public string ImageContainerName { get; set; } = "images";

    /// <summary>
    /// Name of the Azure Blob container for storing multimedia files.
    /// </summary>
    public string MultimediaContainerName { get; set; } = "multimedia";

    // Queue Names
    /// <summary>
    /// Name of the Azure Queue for order processing messages.
    /// </summary>
    public string OrderQueueName { get; set; } = "order-processing";

    /// <summary>
    /// Name of the Azure Queue for inventory management messages.
    /// </summary>
    public string InventoryQueueName { get; set; } = "inventory-management";

    /// <summary>
    /// Name of the Azure Queue for image upload notification messages.
    /// </summary>
    public string ImageNotificationQueueName { get; set; } = "image-notifications";

    // File Share Names
    /// <summary>
    /// Name of the Azure File Share for storing application log files.
    /// </summary>
    public string LogFileShareName { get; set; } = "logs";

    // Helper methods to get the appropriate connection string for each service
    /// <summary>
    /// Gets the effective connection string for Table Storage.
    /// </summary>
    public string GetTableConnectionString() =>
        !string.IsNullOrEmpty(TableConnectionString) ? TableConnectionString : ConnectionString;

    /// <summary>
    /// Gets the effective connection string for Blob Storage.
    /// </summary>
    public string GetBlobConnectionString() =>
        !string.IsNullOrEmpty(BlobConnectionString) ? BlobConnectionString : ConnectionString;

    /// <summary>
    /// Gets the effective connection string for Queue Storage.
    /// </summary>
    public string GetQueueConnectionString() =>
        !string.IsNullOrEmpty(QueueConnectionString) ? QueueConnectionString : ConnectionString;

    /// <summary>
    /// Gets the effective connection string for File Storage.
    /// </summary>
    public string GetFileConnectionString() =>
        !string.IsNullOrEmpty(FileConnectionString) ? FileConnectionString : ConnectionString;
}
