using ABCRetail.Services;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace ABCRetail.Tests.PropertyTests;

/// <summary>
/// Property-based tests for file type validation.
/// **Validates: Requirements 12.3**
/// 
/// Property 7: File Type Validation
/// - If the file extension is in the allowed set (images: .jpeg, .jpg, .png, .gif, .webp; 
///   multimedia: .pdf, .mp4, .docx, .xlsx), the validation SHALL accept the file.
/// - If the file extension is NOT in the allowed set, the validation SHALL reject the file 
///   with an appropriate error message.
/// </summary>
public class FileTypeValidationTests
{
    // Valid image extensions (as per BlobStorageService)
    private static readonly string[] ValidImageExtensions = { ".jpeg", ".jpg", ".png", ".gif", ".webp" };

    // Valid multimedia extensions (as per BlobStorageService)
    private static readonly string[] ValidMultimediaExtensions = { ".pdf", ".mp4", ".docx", ".xlsx" };

    // Invalid extensions that should be rejected
    private static readonly string[] InvalidExtensions = 
    { 
        ".exe", ".bat", ".cmd", ".ps1", ".sh", ".js", ".py", ".php",
        ".html", ".xml", ".json", ".txt", ".csv", ".zip", ".rar",
        ".dll", ".bin", ".com", ".msi", ".app", ".dmg", ".iso",
        ".bmp", ".tiff", ".svg", ".psd", ".ai", ".eps"
    };

    #region Property-Based Tests

    /// <summary>
    /// Property test: All valid image extensions are accepted by IsValidImageType.
    /// **Validates: Requirements 12.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ValidImageExtensions_AreAccepted()
    {
        var extensionGen = Gen.Elements(ValidImageExtensions);
        
        return Prop.ForAll(extensionGen.ToArbitrary(), extension =>
        {
            var result = BlobStorageService.IsValidImageType(extension);
            return result.ToProperty()
                .Label($"Extension '{extension}' should be valid for images");
        });
    }

    /// <summary>
    /// Property test: All valid multimedia extensions are accepted by IsValidMultimediaType.
    /// **Validates: Requirements 12.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ValidMultimediaExtensions_AreAccepted()
    {
        var extensionGen = Gen.Elements(ValidMultimediaExtensions);
        
        return Prop.ForAll(extensionGen.ToArbitrary(), extension =>
        {
            var result = BlobStorageService.IsValidMultimediaType(extension);
            return result.ToProperty()
                .Label($"Extension '{extension}' should be valid for multimedia");
        });
    }

    /// <summary>
    /// Property test: Invalid extensions are rejected by IsValidImageType.
    /// **Validates: Requirements 12.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property InvalidExtensions_AreRejectedForImages()
    {
        var extensionGen = Gen.Elements(InvalidExtensions);
        
        return Prop.ForAll(extensionGen.ToArbitrary(), extension =>
        {
            var result = BlobStorageService.IsValidImageType(extension);
            return (!result).ToProperty()
                .Label($"Extension '{extension}' should be invalid for images");
        });
    }

    /// <summary>
    /// Property test: Invalid extensions are rejected by IsValidMultimediaType.
    /// **Validates: Requirements 12.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property InvalidExtensions_AreRejectedForMultimedia()
    {
        var extensionGen = Gen.Elements(InvalidExtensions);
        
        return Prop.ForAll(extensionGen.ToArbitrary(), extension =>
        {
            var result = BlobStorageService.IsValidMultimediaType(extension);
            return (!result).ToProperty()
                .Label($"Extension '{extension}' should be invalid for multimedia");
        });
    }

    /// <summary>
    /// Property test: Image extensions should NOT be valid for multimedia (except those shared).
    /// **Validates: Requirements 12.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ImageExtensions_NotValidForMultimedia()
    {
        var extensionGen = Gen.Elements(ValidImageExtensions);
        
        return Prop.ForAll(extensionGen.ToArbitrary(), extension =>
        {
            var result = BlobStorageService.IsValidMultimediaType(extension);
            return (!result).ToProperty()
                .Label($"Image extension '{extension}' should be invalid for multimedia");
        });
    }

    /// <summary>
    /// Property test: Multimedia extensions should NOT be valid for images.
    /// **Validates: Requirements 12.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MultimediaExtensions_NotValidForImages()
    {
        var extensionGen = Gen.Elements(ValidMultimediaExtensions);
        
        return Prop.ForAll(extensionGen.ToArbitrary(), extension =>
        {
            var result = BlobStorageService.IsValidImageType(extension);
            return (!result).ToProperty()
                .Label($"Multimedia extension '{extension}' should be invalid for images");
        });
    }

    /// <summary>
    /// Property test: Validation is case-insensitive for image extensions.
    /// **Validates: Requirements 12.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ImageValidation_IsCaseInsensitive()
    {
        var extensionGen = Gen.Elements(ValidImageExtensions);
        
        return Prop.ForAll(extensionGen.ToArbitrary(), extension =>
        {
            var upperResult = BlobStorageService.IsValidImageType(extension.ToUpperInvariant());
            var lowerResult = BlobStorageService.IsValidImageType(extension.ToLowerInvariant());
            var mixedResult = BlobStorageService.IsValidImageType(ToMixedCase(extension));
            
            return (upperResult && lowerResult && mixedResult).ToProperty()
                .Label($"Case variations of '{extension}' should all be valid");
        });
    }

    /// <summary>
    /// Property test: Validation is case-insensitive for multimedia extensions.
    /// **Validates: Requirements 12.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MultimediaValidation_IsCaseInsensitive()
    {
        var extensionGen = Gen.Elements(ValidMultimediaExtensions);
        
        return Prop.ForAll(extensionGen.ToArbitrary(), extension =>
        {
            var upperResult = BlobStorageService.IsValidMultimediaType(extension.ToUpperInvariant());
            var lowerResult = BlobStorageService.IsValidMultimediaType(extension.ToLowerInvariant());
            var mixedResult = BlobStorageService.IsValidMultimediaType(ToMixedCase(extension));
            
            return (upperResult && lowerResult && mixedResult).ToProperty()
                .Label($"Case variations of '{extension}' should all be valid");
        });
    }

    #endregion

    #region Unit Tests for Specific Cases

    /// <summary>
    /// Verifies all individual valid image extensions.
    /// </summary>
    [Theory]
    [InlineData(".jpeg")]
    [InlineData(".jpg")]
    [InlineData(".png")]
    [InlineData(".gif")]
    [InlineData(".webp")]
    public void IsValidImageType_WithValidExtension_ReturnsTrue(string extension)
    {
        // Act
        var result = BlobStorageService.IsValidImageType(extension);

        // Assert
        result.Should().BeTrue($"'{extension}' is a valid image extension");
    }

    /// <summary>
    /// Verifies all individual valid multimedia extensions.
    /// </summary>
    [Theory]
    [InlineData(".pdf")]
    [InlineData(".mp4")]
    [InlineData(".docx")]
    [InlineData(".xlsx")]
    public void IsValidMultimediaType_WithValidExtension_ReturnsTrue(string extension)
    {
        // Act
        var result = BlobStorageService.IsValidMultimediaType(extension);

        // Assert
        result.Should().BeTrue($"'{extension}' is a valid multimedia extension");
    }

    /// <summary>
    /// Verifies that common invalid extensions are rejected for images.
    /// </summary>
    [Theory]
    [InlineData(".exe")]
    [InlineData(".bat")]
    [InlineData(".txt")]
    [InlineData(".pdf")]
    [InlineData(".mp4")]
    [InlineData("")]
    [InlineData(".")]
    public void IsValidImageType_WithInvalidExtension_ReturnsFalse(string extension)
    {
        // Act
        var result = BlobStorageService.IsValidImageType(extension);

        // Assert
        result.Should().BeFalse($"'{extension}' should not be a valid image extension");
    }

    /// <summary>
    /// Verifies that common invalid extensions are rejected for multimedia.
    /// </summary>
    [Theory]
    [InlineData(".exe")]
    [InlineData(".bat")]
    [InlineData(".txt")]
    [InlineData(".jpg")]
    [InlineData(".png")]
    [InlineData("")]
    [InlineData(".")]
    public void IsValidMultimediaType_WithInvalidExtension_ReturnsFalse(string extension)
    {
        // Act
        var result = BlobStorageService.IsValidMultimediaType(extension);

        // Assert
        result.Should().BeFalse($"'{extension}' should not be a valid multimedia extension");
    }

    /// <summary>
    /// Verifies case-insensitive validation for image types.
    /// </summary>
    [Theory]
    [InlineData(".JPEG")]
    [InlineData(".JPG")]
    [InlineData(".PNG")]
    [InlineData(".GIF")]
    [InlineData(".WEBP")]
    [InlineData(".Jpeg")]
    [InlineData(".Jpg")]
    [InlineData(".Png")]
    public void IsValidImageType_WithDifferentCases_ReturnsTrue(string extension)
    {
        // Act
        var result = BlobStorageService.IsValidImageType(extension);

        // Assert
        result.Should().BeTrue($"'{extension}' should be valid regardless of case");
    }

    /// <summary>
    /// Verifies case-insensitive validation for multimedia types.
    /// </summary>
    [Theory]
    [InlineData(".PDF")]
    [InlineData(".MP4")]
    [InlineData(".DOCX")]
    [InlineData(".XLSX")]
    [InlineData(".Pdf")]
    [InlineData(".Mp4")]
    public void IsValidMultimediaType_WithDifferentCases_ReturnsTrue(string extension)
    {
        // Act
        var result = BlobStorageService.IsValidMultimediaType(extension);

        // Assert
        result.Should().BeTrue($"'{extension}' should be valid regardless of case");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Converts a string to mixed case (alternating upper/lower).
    /// </summary>
    private static string ToMixedCase(string input)
    {
        var result = input.ToCharArray();
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = i % 2 == 0 
                ? char.ToUpperInvariant(result[i]) 
                : char.ToLowerInvariant(result[i]);
        }
        return new string(result);
    }

    #endregion
}
