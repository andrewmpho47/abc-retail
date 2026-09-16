namespace ABCRetail.Functions.Models;

public sealed class AzureStorageSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string TableConnectionString { get; set; } = string.Empty;
    public string BlobConnectionString { get; set; } = string.Empty;
    public string QueueConnectionString { get; set; } = string.Empty;
    public string FileConnectionString { get; set; } = string.Empty;
    public string CustomerTableName { get; set; } = "customers";
    public string ProductTableName { get; set; } = "products";
    public string ImageContainerName { get; set; } = "images";
    public string MultimediaContainerName { get; set; } = "multimedia";
    public string OrderQueueName { get; set; } = "order-processing";
    public string InventoryQueueName { get; set; } = "inventory-management";
    public string ImageNotificationQueueName { get; set; } = "image-notifications";
    public string LogFileShareName { get; set; } = "logs";

    public string GetTableConnectionString() => string.IsNullOrWhiteSpace(TableConnectionString) ? ConnectionString : TableConnectionString;
    public string GetBlobConnectionString() => string.IsNullOrWhiteSpace(BlobConnectionString) ? ConnectionString : BlobConnectionString;
    public string GetQueueConnectionString() => string.IsNullOrWhiteSpace(QueueConnectionString) ? ConnectionString : QueueConnectionString;
    public string GetFileConnectionString() => string.IsNullOrWhiteSpace(FileConnectionString) ? ConnectionString : FileConnectionString;
}

public sealed class CustomerTableRequest
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}

public sealed class BlobWriteRequest
{
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public string? Base64Content { get; set; }
}

public sealed class QueueTransactionRequest
{
    public string? OrderId { get; set; }
    public string? CustomerId { get; set; }
    public string? ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? OrderStatus { get; set; }
}

public sealed class FileWriteRequest
{
    public string? FileName { get; set; }
    public string? Content { get; set; }
}
