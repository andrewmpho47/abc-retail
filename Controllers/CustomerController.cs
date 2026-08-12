using Microsoft.AspNetCore.Mvc;
using ABCRetail.Models;
using ABCRetail.Models.ViewModels;
using ABCRetail.Services.Interfaces;

namespace ABCRetail.Controllers;

/// <summary>
/// Controller for managing customer profiles stored in Azure Table Storage.
/// </summary>
public class CustomerController : Controller
{
    private readonly ITableStorageService _tableStorageService;
    private readonly IStorageErrorLogger _errorLogger;
    private const string ServiceName = "TableStorageService";

    public CustomerController(
        ITableStorageService tableStorageService,
        IStorageErrorLogger errorLogger)
    {
        _tableStorageService = tableStorageService;
        _errorLogger = errorLogger;
    }

    /// <summary>
    /// Displays a list of all customers.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var customers = await _tableStorageService.GetAllCustomersAsync();
            return View(customers);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("GetAllCustomers", ServiceName, ex);
            TempData["Error"] = $"Failed to load customers: {ex.Message}";
            return View(Enumerable.Empty<CustomerProfile>());
        }
    }

    /// <summary>
    /// Displays details of a specific customer.
    /// </summary>
    public async Task<IActionResult> Details(string partitionKey, string rowKey)
    {
        if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
        {
            TempData["Error"] = "Customer identifier is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var customer = await _tableStorageService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("GetCustomer", ServiceName, ex, $"PartitionKey: {partitionKey}, RowKey: {rowKey}");
            TempData["Error"] = $"Failed to load customer: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Displays the create customer form.
    /// </summary>
    public IActionResult Create()
    {
        return View(new CustomerFormViewModel());
    }

    /// <summary>
    /// Handles the creation of a new customer.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var customer = new CustomerProfile
            {
                PartitionKey = model.PartitionKey,
                RowKey = model.RowKey,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber ?? string.Empty,
                Address = model.Address ?? string.Empty
            };

            await _tableStorageService.CreateCustomerAsync(customer);
            TempData["Success"] = "Customer created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("CreateCustomer", ServiceName, ex, $"PartitionKey: {model.PartitionKey}, RowKey: {model.RowKey}");
            TempData["Error"] = $"Failed to create customer: {ex.Message}";
            return View(model);
        }
    }

    /// <summary>
    /// Displays the edit customer form.
    /// </summary>
    public async Task<IActionResult> Edit(string partitionKey, string rowKey)
    {
        if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
        {
            TempData["Error"] = "Customer identifier is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var customer = await _tableStorageService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new CustomerFormViewModel
            {
                PartitionKey = customer.PartitionKey,
                RowKey = customer.RowKey,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address
            };

            ViewBag.ETag = customer.ETag.ToString();
            return View(model);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("GetCustomerForEdit", ServiceName, ex, $"PartitionKey: {partitionKey}, RowKey: {rowKey}");
            TempData["Error"] = $"Failed to load customer: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Handles the update of an existing customer.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CustomerFormViewModel model, string etag)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var customer = new CustomerProfile
            {
                PartitionKey = model.PartitionKey,
                RowKey = model.RowKey,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber ?? string.Empty,
                Address = model.Address ?? string.Empty,
                ETag = new Azure.ETag(etag)
            };

            await _tableStorageService.UpdateCustomerAsync(customer);
            TempData["Success"] = "Customer updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("UpdateCustomer", ServiceName, ex, $"PartitionKey: {model.PartitionKey}, RowKey: {model.RowKey}");
            TempData["Error"] = $"Failed to update customer: {ex.Message}";
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
            TempData["Error"] = "Customer identifier is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var customer = await _tableStorageService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("GetCustomerForDelete", ServiceName, ex, $"PartitionKey: {partitionKey}, RowKey: {rowKey}");
            TempData["Error"] = $"Failed to load customer: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Handles the deletion of a customer.
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
    {
        try
        {
            await _tableStorageService.DeleteCustomerAsync(partitionKey, rowKey);
            TempData["Success"] = "Customer deleted successfully.";
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("DeleteCustomer", ServiceName, ex, $"PartitionKey: {partitionKey}, RowKey: {rowKey}");
            TempData["Error"] = $"Failed to delete customer: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
