using System.ComponentModel.DataAnnotations;

namespace ABCRetail.Models.ViewModels;

/// <summary>
/// View model for inventory message creation form.
/// Includes validation attributes for form submission.
/// </summary>
public class InventoryFormViewModel
{
    /// <summary>
    /// Identifier of the product for the inventory action.
    /// </summary>
    [Required(ErrorMessage = "Product ID is required.")]
    [Display(Name = "Product ID")]
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Type of inventory action: "Restock", "Deduct", or "Alert".
    /// </summary>
    [Required(ErrorMessage = "Action Type is required.")]
    [Display(Name = "Action Type")]
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// Quantity of units affected by the inventory action.
    /// </summary>
    [Required(ErrorMessage = "Quantity is required.")]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative value.")]
    [Display(Name = "Quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// Reason or description for the inventory action.
    /// </summary>
    [Required(ErrorMessage = "Reason is required.")]
    [Display(Name = "Reason")]
    public string Reason { get; set; } = string.Empty;
}
