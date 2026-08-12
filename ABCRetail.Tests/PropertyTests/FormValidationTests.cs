using System.ComponentModel.DataAnnotations;
using ABCRetail.Models.ViewModels;
using ABCRetail.Tests.Generators;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace ABCRetail.Tests.PropertyTests;

/// <summary>
/// Property-based tests for form validation.
/// **Validates: Requirements 12.1, 12.2**
/// 
/// Property 6: Form Validation for Required Fields
/// - For any form submission where one or more required fields are empty or contain only whitespace,
///   the validation SHALL reject the submission and display specific error messages identifying 
///   which fields are invalid.
/// </summary>
public class FormValidationTests
{
    /// <summary>
    /// Property test: Valid CustomerFormViewModel passes all validation.
    /// **Validates: Requirements 12.1, 12.2**
    /// </summary>
    [Property(Arbitrary = new[] { typeof(ValidCustomerFormViewModelArbitrary) }, MaxTest = 100)]
    public Property ValidCustomerForm_PassesValidation(CustomerFormViewModel viewModel)
    {
        // Act
        var validationResults = ValidateModel(viewModel);

        // Assert - valid data should have no validation errors
        return (!validationResults.Any()).ToProperty()
            .Label($"Expected no validation errors for valid form, but got {validationResults.Count} errors")
            .Label($"Errors: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}");
    }

    /// <summary>
    /// Property test: CustomerFormViewModel with empty required fields fails validation.
    /// **Validates: Requirements 12.1, 12.2**
    /// </summary>
    [Property(Arbitrary = new[] { typeof(InvalidCustomerFormViewModelArbitrary) }, MaxTest = 100)]
    public Property InvalidCustomerForm_WithEmptyRequiredField_FailsValidation(CustomerFormViewModel viewModel)
    {
        // Act
        var validationResults = ValidateModel(viewModel);

        // Assert - invalid data should have validation errors
        return validationResults.Any().ToProperty()
            .Label($"Expected validation errors for invalid form with empty required field(s)")
            .Label($"PartitionKey: '{viewModel.PartitionKey}'")
            .Label($"RowKey: '{viewModel.RowKey}'")
            .Label($"FirstName: '{viewModel.FirstName}'")
            .Label($"LastName: '{viewModel.LastName}'")
            .Label($"Email: '{viewModel.Email}'");
    }

    /// <summary>
    /// Verifies that empty PartitionKey fails validation with the correct error message.
    /// </summary>
    [Fact]
    public void CustomerFormViewModel_EmptyPartitionKey_FailsValidation()
    {
        // Arrange
        var viewModel = new CustomerFormViewModel
        {
            PartitionKey = "",
            RowKey = "ROW001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        // Act
        var validationResults = ValidateModel(viewModel);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains("PartitionKey"));
        validationResults.Should().Contain(r => r.ErrorMessage!.Contains("Partition Key is required"));
    }

    /// <summary>
    /// Verifies that empty RowKey fails validation with the correct error message.
    /// </summary>
    [Fact]
    public void CustomerFormViewModel_EmptyRowKey_FailsValidation()
    {
        // Arrange
        var viewModel = new CustomerFormViewModel
        {
            PartitionKey = "PART001",
            RowKey = "",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        // Act
        var validationResults = ValidateModel(viewModel);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains("RowKey"));
        validationResults.Should().Contain(r => r.ErrorMessage!.Contains("Row Key is required"));
    }

    /// <summary>
    /// Verifies that empty FirstName fails validation with the correct error message.
    /// </summary>
    [Fact]
    public void CustomerFormViewModel_EmptyFirstName_FailsValidation()
    {
        // Arrange
        var viewModel = new CustomerFormViewModel
        {
            PartitionKey = "PART001",
            RowKey = "ROW001",
            FirstName = "",
            LastName = "Doe",
            Email = "john@example.com"
        };

        // Act
        var validationResults = ValidateModel(viewModel);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains("FirstName"));
        validationResults.Should().Contain(r => r.ErrorMessage!.Contains("First Name is required"));
    }

    /// <summary>
    /// Verifies that empty LastName fails validation with the correct error message.
    /// </summary>
    [Fact]
    public void CustomerFormViewModel_EmptyLastName_FailsValidation()
    {
        // Arrange
        var viewModel = new CustomerFormViewModel
        {
            PartitionKey = "PART001",
            RowKey = "ROW001",
            FirstName = "John",
            LastName = "",
            Email = "john@example.com"
        };

        // Act
        var validationResults = ValidateModel(viewModel);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains("LastName"));
        validationResults.Should().Contain(r => r.ErrorMessage!.Contains("Last Name is required"));
    }

    /// <summary>
    /// Verifies that empty Email fails validation with the correct error message.
    /// </summary>
    [Fact]
    public void CustomerFormViewModel_EmptyEmail_FailsValidation()
    {
        // Arrange
        var viewModel = new CustomerFormViewModel
        {
            PartitionKey = "PART001",
            RowKey = "ROW001",
            FirstName = "John",
            LastName = "Doe",
            Email = ""
        };

        // Act
        var validationResults = ValidateModel(viewModel);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains("Email"));
        validationResults.Should().Contain(r => r.ErrorMessage!.Contains("Email is required"));
    }

    /// <summary>
    /// Verifies that invalid Email format fails validation.
    /// </summary>
    [Theory]
    [InlineData("invalid")]
    [InlineData("missing-at-symbol.com")]
    [InlineData("@nodomain.com")]
    public void CustomerFormViewModel_InvalidEmailFormat_FailsValidation(string invalidEmail)
    {
        // Arrange
        var viewModel = new CustomerFormViewModel
        {
            PartitionKey = "PART001",
            RowKey = "ROW001",
            FirstName = "John",
            LastName = "Doe",
            Email = invalidEmail
        };

        // Act
        var validationResults = ValidateModel(viewModel);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains("Email"));
    }

    /// <summary>
    /// Verifies that FirstName exceeding max length fails validation.
    /// </summary>
    [Fact]
    public void CustomerFormViewModel_FirstNameExceedsMaxLength_FailsValidation()
    {
        // Arrange
        var viewModel = new CustomerFormViewModel
        {
            PartitionKey = "PART001",
            RowKey = "ROW001",
            FirstName = new string('A', 101), // Max is 100
            LastName = "Doe",
            Email = "john@example.com"
        };

        // Act
        var validationResults = ValidateModel(viewModel);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains("FirstName"));
        validationResults.Should().Contain(r => r.ErrorMessage!.Contains("cannot exceed 100 characters"));
    }

    /// <summary>
    /// Verifies that LastName exceeding max length fails validation.
    /// </summary>
    [Fact]
    public void CustomerFormViewModel_LastNameExceedsMaxLength_FailsValidation()
    {
        // Arrange
        var viewModel = new CustomerFormViewModel
        {
            PartitionKey = "PART001",
            RowKey = "ROW001",
            FirstName = "John",
            LastName = new string('D', 101), // Max is 100
            Email = "john@example.com"
        };

        // Act
        var validationResults = ValidateModel(viewModel);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains("LastName"));
        validationResults.Should().Contain(r => r.ErrorMessage!.Contains("cannot exceed 100 characters"));
    }

    /// <summary>
    /// Verifies that multiple empty required fields produce multiple validation errors.
    /// </summary>
    [Fact]
    public void CustomerFormViewModel_MultipleEmptyRequiredFields_ReturnsMultipleErrors()
    {
        // Arrange
        var viewModel = new CustomerFormViewModel
        {
            PartitionKey = "",
            RowKey = "",
            FirstName = "",
            LastName = "",
            Email = ""
        };

        // Act
        var validationResults = ValidateModel(viewModel);

        // Assert
        validationResults.Should().HaveCountGreaterOrEqualTo(5, "all five required fields are empty");
        validationResults.Should().Contain(r => r.MemberNames.Contains("PartitionKey"));
        validationResults.Should().Contain(r => r.MemberNames.Contains("RowKey"));
        validationResults.Should().Contain(r => r.MemberNames.Contains("FirstName"));
        validationResults.Should().Contain(r => r.MemberNames.Contains("LastName"));
        validationResults.Should().Contain(r => r.MemberNames.Contains("Email"));
    }

    /// <summary>
    /// Verifies that optional fields (PhoneNumber, Address) don't cause validation errors when empty.
    /// </summary>
    [Fact]
    public void CustomerFormViewModel_EmptyOptionalFields_PassesValidation()
    {
        // Arrange
        var viewModel = new CustomerFormViewModel
        {
            PartitionKey = "PART001",
            RowKey = "ROW001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "", // Optional
            Address = ""     // Optional
        };

        // Act
        var validationResults = ValidateModel(viewModel);

        // Assert
        validationResults.Should().BeEmpty("optional fields should not cause validation errors when empty");
    }

    /// <summary>
    /// Helper method to validate a model using DataAnnotations.
    /// </summary>
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
        return validationResults;
    }
}
