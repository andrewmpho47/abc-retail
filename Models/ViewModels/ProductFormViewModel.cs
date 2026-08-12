using System.ComponentModel.DataAnnotations;

namespace ABCRetail.Models.ViewModels;

/// <summary>
/// View model for product create and edit forms.
/// Includes validation attributes for form submission.
/// </summary>
public class ProductFormViewModel
{
    /// <summary>
    /// Product category (used as PartitionKey in Azure Table Storage).
    /// </summary>
    [Required(ErrorMessage = "Category is required.")]
    [Display(Name = "Category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Unique product identifier (used as RowKey in Azure Table Storage).
    /// </summary>
    [Required(ErrorMessage = "Product ID is required.")]
    [Display(Name = "Product ID")]
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the product.
    /// </summary>
    [Required(ErrorMessage = "Product Name is required.")]
    [Display(Name = "Product Name")]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the product.
    /// </summary>
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Price of the product.
    /// </summary>
    [Required(ErrorMessage = "Price is required.")]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be a non-negative value.")]
    [Display(Name = "Price")]
    public double Price { get; set; }

    /// <summary>
    /// Current stock quantity available.
    /// </summary>
    [Required(ErrorMessage = "Stock Quantity is required.")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock Quantity must be a non-negative value.")]
    [Display(Name = "Stock Quantity")]
    public int StockQuantity { get; set; }
}
