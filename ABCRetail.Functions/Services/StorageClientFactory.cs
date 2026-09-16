using ABCRetail.Functions.Models;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;

namespace ABCRetail.Functions;

public sealed class StorageClientFactory
{
    private readonly AzureStorageSettings _settings;

    public StorageClientFactory(AzureStorageSettings settings)
    {
        _settings = settings;
    }

    public TableClient CustomerTable
    {
        get
        {
            var serviceClient = new TableServiceClient(_settings.GetTableConnectionString());
            var tableClient = serviceClient.GetTableClient(_settings.CustomerTableName);
            tableClient.CreateIfNotExists();
            return tableClient;
        }
    }

    public BlobContainerClient ImageContainer
    {
        get
        {
            var serviceClient = new BlobServiceClient(_settings.GetBlobConnectionString());
            var containerClient = serviceClient.GetBlobContainerClient(_settings.ImageContainerName);
            containerClient.CreateIfNotExists();
            return containerClient;
        }
    }

    public QueueClient OrderQueue
    {
        get
        {
            var queueClient = new QueueClient(_settings.GetQueueConnectionString(), _settings.OrderQueueName);
            queueClient.CreateIfNotExists();
            return queueClient;
        }
    }

    public ShareDirectoryClient LogFileRootDirectory
    {
        get
        {
            var shareClient = new ShareClient(_settings.GetFileConnectionString(), _settings.LogFileShareName);
            shareClient.CreateIfNotExists();
            return shareClient.GetRootDirectoryClient();
        }
    }
}
