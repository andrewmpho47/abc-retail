using System.ComponentModel.DataAnnotations;

namespace ABCRetail.Models.ViewModels;

/// <summary>
/// View model for customer profile create and edit forms.
/// Includes validation attributes for form submission.
/// </summary>
public class CustomerFormViewModel
{
    /// <summary>
    /// Partition key for logical grouping of customer records.
    /// </summary>
    [Required(ErrorMessage = "Partition Key is required.")]
    [Display(Name = "Partition Key")]
    public string PartitionKey { get; set; } = string.Empty;

    /// <summary>
    /// Row key serving as the unique customer identifier.
    /// </summary>
    [Required(ErrorMessage = "Row Key is required.")]
    [Display(Name = "Row Key")]
    public string RowKey { get; set; } = string.Empty;

    /// <summary>
    /// Customer's first name.
    /// </summary>
    [Required(ErrorMessage = "First Name is required.")]
    [StringLength(100, ErrorMessage = "First Name cannot exceed 100 characters.")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Customer's last name.
    /// </summary>
    [Required(ErrorMessage = "Last Name is required.")]
    [StringLength(100, ErrorMessage = "Last Name cannot exceed 100 characters.")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Customer's email address.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Customer's phone number.
    /// </summary>
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Customer's physical address.
    /// </summary>
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;
}
