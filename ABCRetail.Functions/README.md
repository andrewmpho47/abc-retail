# ABC Retail Azure Functions

This project supports CLDV7112 Project 2 by adding four Azure Functions that integrate with the same Azure Storage account used by the MVC web application.

## Functions

| Function | Route | Method | Assessment requirement |
| --- | --- | --- | --- |
| `StoreCustomerInTable` | `/api/tables/customers` | `POST` | Stores customer information in Azure Table Storage |
| `WriteImageToBlobStorage` | `/api/blobs/images` | `POST` | Writes an image file to Azure Blob Storage |
| `ReadWriteOrderQueue` | `/api/queues/orders` | `GET`, `POST`, `DELETE` | Writes, reads, and processes order transaction messages in Azure Queue Storage |
| `SendFileToAzureFiles` | `/api/files/logs` | `POST` | Sends a text log file to Azure Files |

## Local configuration

Copy `local.settings.example.json` to `local.settings.json`, then replace `UseDevelopmentStorage=true` with your Azure Storage connection string if you want to test against real Azure Storage.

`local.settings.json` is intentionally ignored by Git because it can contain secrets.

## Azure Function App settings

Add these settings in the Azure Function App Configuration page:

| Name | Value |
| --- | --- |
| `AzureWebJobsStorage` | Your Azure Storage connection string |
| `FUNCTIONS_WORKER_RUNTIME` | `dotnet-isolated` |
| `AzureStorageSettings__ConnectionString` | Your Azure Storage connection string |
| `AzureStorageSettings__CustomerTableName` | `customers` |
| `AzureStorageSettings__ProductTableName` | `products` |
| `AzureStorageSettings__ImageContainerName` | `images` |
| `AzureStorageSettings__MultimediaContainerName` | `multimedia` |
| `AzureStorageSettings__OrderQueueName` | `order-processing` |
| `AzureStorageSettings__InventoryQueueName` | `inventory-management` |
| `AzureStorageSettings__ImageNotificationQueueName` | `image-notifications` |
| `AzureStorageSettings__LogFileShareName` | `logs` |

## Sample requests

Use the Azure portal function Test/Run panel, Postman, or curl. In Azure, include the function key if prompted.

### Store customer in Table Storage

```http
POST /api/tables/customers
Content-Type: application/json

{
  "partitionKey": "Customer",
  "firstName": "Ava",
  "lastName": "Mokoena",
  "email": "ava@example.com",
  "phoneNumber": "0820000000",
  "address": "Cape Town"
}
```

### Write image to Blob Storage

```http
POST /api/blobs/images
Content-Type: application/json

{
  "fileName": "project2-evidence.png"
}
```

The function writes a tiny valid PNG if no `base64Content` is supplied. You can also upload an image through multipart form data with a `file` field.

### Write and read queue messages

```http
POST /api/queues/orders
Content-Type: application/json

{
  "orderId": "ORD-1001",
  "customerId": "Customer-001",
  "productId": "Product-001",
  "quantity": 2,
  "orderStatus": "Submitted"
}
```

```http
GET /api/queues/orders
```

```http
DELETE /api/queues/orders
```

### Send a file to Azure Files

```http
POST /api/files/logs
Content-Type: application/json

{
  "fileName": "project2-function-log.txt",
  "content": "Azure Files evidence from the ABC Retail Function App."
}
```
