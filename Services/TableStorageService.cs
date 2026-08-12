using Azure;
using Azure.Data.Tables;
using ABCRetail.Models;
using ABCRetail.Services.Interfaces;

namespace ABCRetail.Services;

/// <summary>
/// Implements Azure Table Storage operations for customer profiles and products.
/// </summary>
public class TableStorageService : ITableStorageService
{
    private readonly TableClient _customerTableClient;
    private readonly TableClient _productTableClient;

    /// <summary>
    /// Initializes a new instance of the TableStorageService.
    /// </summary>
    /// <param name="settings">The Azure Storage settings containing connection strings and table names.</param>
    public TableStorageService(AzureStorageSettings settings)
    {
        var connectionString = settings.GetTableConnectionString();
        
        // Create TableServiceClient for managing tables
        var tableServiceClient = new TableServiceClient(connectionString);
        
        // Get or create the customer table
        _customerTableClient = tableServiceClient.GetTableClient(settings.CustomerTableName);
        _customerTableClient.CreateIfNotExists();
        
        // Get or create the product table
        _productTableClient = tableServiceClient.GetTableClient(settings.ProductTableName);
        _productTableClient.CreateIfNotExists();
    }

    #region Customer Operations

    /// <inheritdoc />
    public async Task<IEnumerable<CustomerProfile>> GetAllCustomersAsync()
    {
        var customers = new List<CustomerProfile>();
        
        try
        {
            await foreach (var customer in _customerTableClient.QueryAsync<CustomerProfile>())
            {
                customers.Add(customer);
            }
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve customers: {ex.Message}", ex);
        }
        
        return customers;
    }

    /// <inheritdoc />
    public async Task<CustomerProfile?> GetCustomerAsync(string partitionKey, string rowKey)
    {
        try
        {
            var response = await _customerTableClient.GetEntityAsync<CustomerProfile>(partitionKey, rowKey);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve customer: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task CreateCustomerAsync(CustomerProfile customer)
    {
        try
        {
            await _customerTableClient.AddEntityAsync(customer);
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            throw new InvalidOperationException("A customer with this key already exists.", ex);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to create customer: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task UpdateCustomerAsync(CustomerProfile customer)
    {
        try
        {
            await _customerTableClient.UpdateEntityAsync(customer, customer.ETag, TableUpdateMode.Replace);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new InvalidOperationException("Customer not found.", ex);
        }
        catch (RequestFailedException ex) when (ex.Status == 412)
        {
            throw new InvalidOperationException("The customer has been modified by another user. Please refresh and try again.", ex);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to update customer: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
    {
        try
        {
            await _customerTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new InvalidOperationException("Customer not found.", ex);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to delete customer: {ex.Message}", ex);
        }
    }

    #endregion

    #region Product Operations

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        var products = new List<Product>();
        
        try
        {
            await foreach (var product in _productTableClient.QueryAsync<Product>())
            {
                products.Add(product);
            }
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve products: {ex.Message}", ex);
        }
        
        return products;
    }

    /// <inheritdoc />
    public async Task<Product?> GetProductAsync(string partitionKey, string rowKey)
    {
        try
        {
            var response = await _productTableClient.GetEntityAsync<Product>(partitionKey, rowKey);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve product: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task CreateProductAsync(Product product)
    {
        try
        {
            await _productTableClient.AddEntityAsync(product);
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            throw new InvalidOperationException("A product with this key already exists.", ex);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to create product: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task UpdateProductAsync(Product product)
    {
        try
        {
            await _productTableClient.UpdateEntityAsync(product, product.ETag, TableUpdateMode.Replace);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new InvalidOperationException("Product not found.", ex);
        }
        catch (RequestFailedException ex) when (ex.Status == 412)
        {
            throw new InvalidOperationException("The product has been modified by another user. Please refresh and try again.", ex);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to update product: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task DeleteProductAsync(string partitionKey, string rowKey)
    {
        try
        {
            await _productTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new InvalidOperationException("Product not found.", ex);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Failed to delete product: {ex.Message}", ex);
        }
    }

    #endregion
}
