using Microsoft.AspNetCore.Mvc;
using ABCRetail.Models;
using ABCRetail.Models.ViewModels;
using ABCRetail.Services.Interfaces;

namespace ABCRetail.Controllers;

/// <summary>
/// Controller for managing inventory management queue in Azure Queue Storage.
/// </summary>
public class InventoryQueueController : Controller
{
    private readonly IQueueStorageService _queueStorageService;
    private readonly IStorageErrorLogger _errorLogger;
    private const string ServiceName = "QueueStorageService";

    public InventoryQueueController(
        IQueueStorageService queueStorageService,
        IStorageErrorLogger errorLogger)
    {
        _queueStorageService = queueStorageService;
        _errorLogger = errorLogger;
    }

    /// <summary>
    /// Displays the inventory queue with messages and count.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var messages = await _queueStorageService.PeekInventoryMessagesAsync();
            var count = await _queueStorageService.GetInventoryQueueCountAsync();
            ViewBag.MessageCount = count;
            return View(messages);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("PeekInventoryMessages", ServiceName, ex);
            TempData["Error"] = $"Failed to load inventory queue: {ex.Message}";
            return View(Enumerable.Empty<QueueMessageInfo>());
        }
    }

    /// <summary>
    /// Displays the send message form.
    /// </summary>
    public IActionResult Send()
    {
        return View(new InventoryFormViewModel());
    }

    /// <summary>
    /// Handles sending a new inventory message to the queue.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(InventoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var inventoryMessage = new InventoryMessage
            {
                ProductId = model.ProductId,
                ActionType = model.ActionType,
                Quantity = model.Quantity,
                Reason = model.Reason
            };

            await _queueStorageService.SendInventoryMessageAsync(inventoryMessage);
            TempData["Success"] = $"Inventory message sent successfully: {inventoryMessage.ToQueueMessage()}";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("SendInventoryMessage", ServiceName, ex, $"ProductId: {model.ProductId}");
            TempData["Error"] = $"Failed to send inventory message: {ex.Message}";
            return View(model);
        }
    }

    /// <summary>
    /// Processes (dequeues) the next message from the queue.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Process()
    {
        try
        {
            var message = await _queueStorageService.DequeueInventoryMessageAsync();
            if (message == null)
            {
                TempData["Error"] = "No messages in the queue to process.";
            }
            else
            {
                TempData["Success"] = $"Processed message: {message.Content}";
            }
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("DequeueInventoryMessage", ServiceName, ex);
            TempData["Error"] = $"Failed to process message: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
