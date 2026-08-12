using Microsoft.AspNetCore.Mvc;
using ABCRetail.Models;
using ABCRetail.Models.ViewModels;
using ABCRetail.Services.Interfaces;

namespace ABCRetail.Controllers;

/// <summary>
/// Controller for managing order processing queue in Azure Queue Storage.
/// </summary>
public class OrderQueueController : Controller
{
    private readonly IQueueStorageService _queueStorageService;
    private readonly IStorageErrorLogger _errorLogger;
    private const string ServiceName = "QueueStorageService";

    public OrderQueueController(
        IQueueStorageService queueStorageService,
        IStorageErrorLogger errorLogger)
    {
        _queueStorageService = queueStorageService;
        _errorLogger = errorLogger;
    }

    /// <summary>
    /// Displays the order queue with messages and count.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var messages = await _queueStorageService.PeekOrderMessagesAsync();
            var count = await _queueStorageService.GetOrderQueueCountAsync();
            ViewBag.MessageCount = count;
            return View(messages);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("PeekOrderMessages", ServiceName, ex);
            TempData["Error"] = $"Failed to load order queue: {ex.Message}";
            return View(Enumerable.Empty<QueueMessageInfo>());
        }
    }

    /// <summary>
    /// Displays the send message form.
    /// </summary>
    public IActionResult Send()
    {
        return View(new OrderFormViewModel());
    }

    /// <summary>
    /// Handles sending a new order message to the queue.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(OrderFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var orderMessage = new OrderMessage
            {
                OrderId = model.OrderId,
                CustomerId = model.CustomerId,
                ProductId = model.ProductId,
                Quantity = model.Quantity,
                OrderStatus = model.OrderStatus
            };

            await _queueStorageService.SendOrderMessageAsync(orderMessage);
            TempData["Success"] = $"Order message sent successfully: {orderMessage.ToQueueMessage()}";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await _errorLogger.LogStorageErrorAsync("SendOrderMessage", ServiceName, ex, $"OrderId: {model.OrderId}");
            TempData["Error"] = $"Failed to send order message: {ex.Message}";
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
            var message = await _queueStorageService.DequeueOrderMessageAsync();
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
            await _errorLogger.LogStorageErrorAsync("DequeueOrderMessage", ServiceName, ex);
            TempData["Error"] = $"Failed to process message: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
