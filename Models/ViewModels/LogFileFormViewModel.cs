using System.ComponentModel.DataAnnotations;

namespace ABCRetail.Models.ViewModels;

/// <summary>
/// View model for log file creation form.
/// Includes validation attributes for form submission.
/// </summary>
public class LogFileFormViewModel
{
    /// <summary>
    /// Name of the log file to create.
    /// </summary>
    [Required(ErrorMessage = "File Name is required.")]
    [Display(Name = "File Name")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Content to write to the log file.
    /// </summary>
    [Required(ErrorMessage = "Content is required.")]
    [Display(Name = "Content")]
    public string Content { get; set; } = string.Empty;
}
