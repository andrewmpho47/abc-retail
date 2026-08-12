using Microsoft.AspNetCore.Mvc;
using ABCRetail.Models;
using ABCRetail.Models.ViewModels;
using ABCRetail.Services.Interfaces;

namespace ABCRetail.Controllers;

/// <summary>
/// Controller for managing products stored in Azure Table Storage.
/// </summary>
public class ProductController : Controller
{
    private readonly ITableStorageService _tableStorageService;
    private readonly IStorageErrorLogger _errorLogger;
    private const string ServiceName = "TableStorageService";

    public ProductController(
        ITableStorageService tableStorageService,
        IStorageErrorLogger errorLogger)
    {
        _tableStorageService = tableStorageService;
        _errorLogger = errorLogger;
    }

    /// <summary>
    /// Displays a list of all products.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var products = await _tableStorageService.GetAllProductsAsync();
            return View(products);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("GetAllProducts", ServiceName, ex);
            TempData["Error"] = $"Failed to load products: {ex.Message}";
            return View(Enumerable.Empty<Product>());
        }
    }

    /// <summary>
    /// Displays details of a specific product.
    /// </summary>
    public async Task<IActionResult> Details(string partitionKey, string rowKey)
    {
        if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
        {
            TempData["Error"] = "Product identifier is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("GetProduct", ServiceName, ex, $"PartitionKey: {partitionKey}, RowKey: {rowKey}");
            TempData["Error"] = $"Failed to load product: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Displays the create product form.
    /// </summary>
    public IActionResult Create()
    {
        return View(new ProductFormViewModel());
    }

    /// <summary>
    /// Handles the creation of a new product.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var product = new Product
            {
                PartitionKey = model.Category,
                RowKey = model.ProductId,
                ProductName = model.ProductName,
                Description = model.Description ?? string.Empty,
                Price = model.Price,
                StockQuantity = model.StockQuantity
            };

            await _tableStorageService.CreateProductAsync(product);
            TempData["Success"] = "Product created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("CreateProduct", ServiceName, ex, $"Category: {model.Category}, ProductId: {model.ProductId}");
            TempData["Error"] = $"Failed to create product: {ex.Message}";
            return View(model);
        }
    }

    /// <summary>
    /// Displays the edit product form.
    /// </summary>
    public async Task<IActionResult> Edit(string partitionKey, string rowKey)
    {
        if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
        {
            TempData["Error"] = "Product identifier is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new ProductFormViewModel
            {
                Category = product.PartitionKey,
                ProductId = product.RowKey,
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity
            };

            ViewBag.ETag = product.ETag.ToString();
            return View(model);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("GetProductForEdit", ServiceName, ex, $"PartitionKey: {partitionKey}, RowKey: {rowKey}");
            TempData["Error"] = $"Failed to load product: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Handles the update of an existing product.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductFormViewModel model, string etag)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var product = new Product
            {
                PartitionKey = model.Category,
                RowKey = model.ProductId,
                ProductName = model.ProductName,
                Description = model.Description ?? string.Empty,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                ETag = new Azure.ETag(etag)
            };

            await _tableStorageService.UpdateProductAsync(product);
            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("UpdateProduct", ServiceName, ex, $"Category: {model.Category}, ProductId: {model.ProductId}");
            TempData["Error"] = $"Failed to update product: {ex.Message}";
            return View(model);
        }
    }

    /// <summary>
    /// Displays the delete confirmation page.
    /// </summary>
    public async Task<IActionResult> Delete(string partitionKey, string rowKey)
    {
        if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
        {
            TempData["Error"] = "Product identifier is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("GetProductForDelete", ServiceName, ex, $"PartitionKey: {partitionKey}, RowKey: {rowKey}");
            TempData["Error"] = $"Failed to load product: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Handles the deletion of a product.
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
    {
        try
        {
            await _tableStorageService.DeleteProductAsync(partitionKey, rowKey);
            TempData["Success"] = "Product deleted successfully.";
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("DeleteProduct", ServiceName, ex, $"PartitionKey: {partitionKey}, RowKey: {rowKey}");
            TempData["Error"] = $"Failed to delete product: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
