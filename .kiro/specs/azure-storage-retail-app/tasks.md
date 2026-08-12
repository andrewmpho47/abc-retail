# Implementation Tasks

## Task 1: Project Setup and Configuration

- [x] 1.1 Create ASP.NET Core MVC project with .NET 8
- [x] 1.2 Install Azure Storage NuGet packages (Azure.Data.Tables, Azure.Storage.Blobs, Azure.Storage.Queues, Azure.Storage.Files.Shares)
- [x] 1.3 Configure Tailwind CSS using CDN or npm build process
- [x] 1.4 Create AzureStorageSettings configuration class
- [x] 1.5 Configure appsettings.json with Azure Storage connection strings and container/table/queue names
- [x] 1.6 Set up dependency injection for storage services in Program.cs

## Task 2: Data Models and Entities

- [x] 2.1 Create CustomerProfile entity implementing ITableEntity
- [x] 2.2 Create Product entity implementing ITableEntity
- [x] 2.3 Create BlobItemInfo model for blob metadata
- [x] 2.4 Create OrderMessage model with ToQueueMessage() method
- [x] 2.5 Create InventoryMessage model with ToQueueMessage() method
- [x] 2.6 Create QueueMessageInfo model for queue message display
- [x] 2.7 Create LogFileInfo model for file metadata
- [x] 2.8 Create view models (CustomerFormViewModel, ProductFormViewModel, OrderFormViewModel, InventoryFormViewModel, LogFileFormViewModel)
- [x] 2.9 Add data annotation validation attributes to view models

## Task 3: Table Storage Service

- [x] 3.1 Create ITableStorageService interface with customer and product CRUD methods
- [x] 3.2 Implement TableStorageService class with Azure.Data.Tables SDK
- [x] 3.3 Implement GetAllCustomersAsync and GetCustomerAsync methods
- [x] 3.4 Implement CreateCustomerAsync, UpdateCustomerAsync, DeleteCustomerAsync methods
- [x] 3.5 Implement GetAllProductsAsync and GetProductAsync methods
- [x] 3.6 Implement CreateProductAsync, UpdateProductAsync, DeleteProductAsync methods
- [x] 3.7 Add error handling with StorageOperationResult pattern
- [x] 3.8 Register TableStorageService in dependency injection

## Task 4: Blob Storage Service

- [x] 4.1 Create IBlobStorageService interface with image and multimedia methods
- [x] 4.2 Implement BlobStorageService class with Azure.Storage.Blobs SDK
- [x] 4.3 Implement UploadImageAsync with unique blob name generation and metadata
- [x] 4.4 Implement GetAllImagesAsync to list blobs with metadata
- [x] 4.5 Implement DownloadImageAsync and DeleteImageAsync methods
- [x] 4.6 Implement GetImageUrlAsync for generating blob URLs
- [x] 4.7 Implement UploadMultimediaAsync for multimedia container
- [x] 4.8 Implement GetAllMultimediaAsync, DownloadMultimediaAsync, DeleteMultimediaAsync
- [x] 4.9 Add file type validation (JPEG, PNG, GIF, WebP for images; PDF, MP4, DOCX, XLSX for multimedia)
- [x] 4.10 Add file size validation
- [x] 4.11 Register BlobStorageService in dependency injection

## Task 5: Queue Storage Service

- [x] 5.1 Create IQueueStorageService interface with order, inventory, and notification methods
- [x] 5.2 Implement QueueStorageService class with Azure.Storage.Queues SDK
- [x] 5.3 Implement SendOrderMessageAsync with formatted message
- [x] 5.4 Implement PeekOrderMessagesAsync and DequeueOrderMessageAsync
- [x] 5.5 Implement GetOrderQueueCountAsync for approximate count
- [x] 5.6 Implement SendInventoryMessageAsync with formatted message
- [x] 5.7 Implement PeekInventoryMessagesAsync and DequeueInventoryMessageAsync
- [x] 5.8 Implement GetInventoryQueueCountAsync for approximate count
- [x] 5.9 Implement SendImageUploadNotificationAsync with formatted message
- [x] 5.10 Implement PeekImageNotificationsAsync
- [x] 5.11 Register QueueStorageService in dependency injection

## Task 6: File Storage Service

- [x] 6.1 Create IFileStorageService interface with log file methods
- [x] 6.2 Implement FileStorageService class with Azure.Storage.Files.Shares SDK
- [x] 6.3 Implement CreateLogFileAsync to write file content
- [x] 6.4 Implement GetAllLogFilesAsync to list files with metadata
- [x] 6.5 Implement GetLogFileContentAsync to read file content
- [x] 6.6 Implement DownloadLogFileAsync for file download stream
- [x] 6.7 Implement DeleteLogFileAsync
- [x] 6.8 Implement AppendToLogFileAsync for error logging
- [x] 6.9 Register FileStorageService in dependency injection

## Task 7: Controllers - Customer and Product

- [x] 7.1 Create CustomerController with Index, Details, Create, Edit, Delete actions
- [x] 7.2 Implement customer CRUD views using Razor and Tailwind CSS
- [x] 7.3 Add form validation and error message display for customers
- [x] 7.4 Create ProductController with Index, Details, Create, Edit, Delete actions
- [x] 7.5 Implement product CRUD views using Razor and Tailwind CSS
- [x] 7.6 Add form validation and error message display for products

## Task 8: Controllers - Blob Storage

- [x] 8.1 Create ImageController with Index, Upload, Download, Delete actions
- [x] 8.2 Implement image gallery view with thumbnails using Tailwind CSS
- [x] 8.3 Implement image upload form with file type validation
- [x] 8.4 Integrate image upload with queue notification service
- [x] 8.5 Create MultimediaController with Index, Upload, Download, Delete actions
- [x] 8.6 Implement multimedia list view with file details
- [x] 8.7 Implement multimedia upload form with file type validation

## Task 9: Controllers - Queue Storage

- [x] 9.1 Create OrderQueueController with Index, Send, Process actions
- [x] 9.2 Implement order queue view showing messages and count
- [x] 9.3 Implement order message form with validation
- [x] 9.4 Create InventoryQueueController with Index, Send, Process actions
- [x] 9.5 Implement inventory queue view showing messages and count
- [x] 9.6 Implement inventory message form with ActionType dropdown (Restock/Deduct/Alert)
- [x] 9.7 Add image upload notifications display to ImageController

## Task 10: Controllers - File Storage

- [x] 10.1 Create LogController with Index, Create, View, Download, Delete actions
- [x] 10.2 Implement log files list view with file metadata
- [x] 10.3 Implement log file creation form
- [x] 10.4 Implement log file content viewer
- [x] 10.5 Add download functionality for log files

## Task 11: Layout and Navigation

- [x] 11.1 Create shared _Layout.cshtml with Tailwind CSS styling
- [x] 11.2 Implement responsive navigation menu with all section links
- [x] 11.3 Create _ViewImports.cshtml and _ViewStart.cshtml
- [x] 11.4 Implement consistent table styling component
- [x] 11.5 Implement consistent form styling component
- [x] 11.6 Implement success/error message partial views with green/red styling
- [x] 11.7 Create HomeController and dashboard Index view

## Task 12: Error Handling and Logging

- [x] 12.1 Create StorageOperationResult<T> class for operation results
- [x] 12.2 Implement GlobalExceptionMiddleware for unhandled exceptions
- [x] 12.3 Register middleware in Program.cs
- [x] 12.4 Implement error logging to Azure Files for all storage operations
- [x] 12.5 Add TempData-based success/error message display in views

## Task 13: Testing Setup

- [x] 13.1 Create test project (xUnit)
- [x] 13.2 Install FsCheck.Xunit, Moq, and FluentAssertions packages
- [x] 13.3 Create test data generators for domain objects
- [x] 13.4 Implement Property 4: Queue message formatting property test (OrderMessage, InventoryMessage, ImageNotification)
- [x] 13.5 Implement Property 6: Form validation property test
- [x] 13.6 Implement Property 7: File type validation property test
- [x] 13.7 Create unit tests for TableStorageService with mocked clients
- [x] 13.8 Create unit tests for BlobStorageService with mocked clients
- [x] 13.9 Create unit tests for QueueStorageService with mocked clients
- [x] 13.10 Create unit tests for FileStorageService with mocked clients

## Task 14: Deployment Configuration

- [x] 14.1 Configure Azure App Service deployment settings
- [x] 14.2 Create deployment-specific appsettings.Production.json
- [x] 14.3 Document Azure App Service configuration for connection strings
- [x] 14.4 Add health check endpoint for deployment verification
- [x] 14.5 Test deployment to Azure App Service
