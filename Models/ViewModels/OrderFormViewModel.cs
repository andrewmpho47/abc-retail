using System.ComponentModel.DataAnnotations;

namespace ABCRetail.Models.ViewModels;

/// <summary>
/// View model for order message creation form.
/// Includes validation attributes for form submission.
/// </summary>
public class OrderFormViewModel
{
    /// <summary>
    /// Unique identifier for the order.
    /// </summary>
    [Required(ErrorMessage = "Order ID is required.")]
    [Display(Name = "Order ID")]
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the customer placing the order.
    /// </summary>
    [Required(ErrorMessage = "Customer ID is required.")]
    [Display(Name = "Customer ID")]
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the product being ordered.
    /// </summary>
    [Required(ErrorMessage = "Product ID is required.")]
    [Display(Name = "Product ID")]
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Quantity of the product being ordered.
    /// </summary>
    [Required(ErrorMessage = "Quantity is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    [Display(Name = "Quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// Current status of the order.
    /// </summary>
    [Required(ErrorMessage = "Order Status is required.")]
    [Display(Name = "Order Status")]
    public string OrderStatus { get; set; } = string.Empty;
}
