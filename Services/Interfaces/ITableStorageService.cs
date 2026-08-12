using ABCRetail.Models;

namespace ABCRetail.Services.Interfaces;

/// <summary>
/// Defines operations for Azure Table Storage interactions for customer profiles and products.
/// </summary>
public interface ITableStorageService
{
    // Customer Profile Operations
    
    /// <summary>
    /// Retrieves all customer profiles from Azure Table Storage.
    /// </summary>
    /// <returns>A collection of all customer profiles.</returns>
    Task<IEnumerable<CustomerProfile>> GetAllCustomersAsync();
    
    /// <summary>
    /// Retrieves a specific customer profile by partition key and row key.
    /// </summary>
    /// <param name="partitionKey">The partition key of the customer.</param>
    /// <param name="rowKey">The row key (unique identifier) of the customer.</param>
    /// <returns>The customer profile if found; otherwise, null.</returns>
    Task<CustomerProfile?> GetCustomerAsync(string partitionKey, string rowKey);
    
    /// <summary>
    /// Creates a new customer profile in Azure Table Storage.
    /// </summary>
    /// <param name="customer">The customer profile to create.</param>
    Task CreateCustomerAsync(CustomerProfile customer);
    
    /// <summary>
    /// Updates an existing customer profile in Azure Table Storage.
    /// </summary>
    /// <param name="customer">The customer profile with updated data.</param>
    Task UpdateCustomerAsync(CustomerProfile customer);
    
    /// <summary>
    /// Deletes a customer profile from Azure Table Storage.
    /// </summary>
    /// <param name="partitionKey">The partition key of the customer to delete.</param>
    /// <param name="rowKey">The row key of the customer to delete.</param>
    Task DeleteCustomerAsync(string partitionKey, string rowKey);
    
    // Product Operations
    
    /// <summary>
    /// Retrieves all products from Azure Table Storage.
    /// </summary>
    /// <returns>A collection of all products.</returns>
    Task<IEnumerable<Product>> GetAllProductsAsync();
    
    /// <summary>
    /// Retrieves a specific product by partition key (category) and row key (product ID).
    /// </summary>
    /// <param name="partitionKey">The partition key (category) of the product.</param>
    /// <param name="rowKey">The row key (product ID) of the product.</param>
    /// <returns>The product if found; otherwise, null.</returns>
    Task<Product?> GetProductAsync(string partitionKey, string rowKey);
    
    /// <summary>
    /// Creates a new product in Azure Table Storage.
    /// </summary>
    /// <param name="product">The product to create.</param>
    Task CreateProductAsync(Product product);
    
    /// <summary>
    /// Updates an existing product in Azure Table Storage.
    /// </summary>
    /// <param name="product">The product with updated data.</param>
    Task UpdateProductAsync(Product product);
    
    /// <summary>
    /// Deletes a product from Azure Table Storage.
    /// </summary>
    /// <param name="partitionKey">The partition key (category) of the product to delete.</param>
    /// <param name="rowKey">The row key (product ID) of the product to delete.</param>
    Task DeleteProductAsync(string partitionKey, string rowKey);
}
