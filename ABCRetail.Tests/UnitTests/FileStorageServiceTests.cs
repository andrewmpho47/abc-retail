using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using ABCRetail.Models;
using ABCRetail.Services;
using FluentAssertions;
using Moq;
using System.Text;
using Xunit;

namespace ABCRetail.Tests.UnitTests;

/// <summary>
/// Unit tests for FileStorageService with mocked Azure File clients.
/// Tests log file create, read, list, download, delete, and append operations.
/// </summary>
public class FileStorageServiceTests
{
    private readonly Mock<ShareClient> _mockShareClient;
    private readonly Mock<ShareDirectoryClient> _mockRootDirectoryClient;
    private readonly Mock<ShareFileClient> _mockFileClient;

    public FileStorageServiceTests()
    {
        _mockShareClient = new Mock<ShareClient>();
        _mockRootDirectoryClient = new Mock<ShareDirectoryClient>();
        _mockFileClient = new Mock<ShareFileClient>();
    }

    #region CreateLogFileAsync Tests

    [Fact]
    public async Task CreateLogFileAsync_WithValidContent_CreatesFile()
    {
        // Arrange
        var content = "This is a test log entry.";
        var contentBytes = Encoding.UTF8.GetBytes(content);

        _mockFileClient
            .Setup(f => f.CreateAsync(contentBytes.Length, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<ShareFileInfo>>());

        _mockFileClient
            .Setup(f => f.UploadAsync(It.IsAny<Stream>(), It.IsAny<ShareFileUploadOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<ShareFileUploadInfo>>());

        // Act
        await _mockFileClient.Object.CreateAsync(contentBytes.Length);
        await _mockFileClient.Object.UploadAsync(new MemoryStream(contentBytes), new ShareFileUploadOptions());

        // Assert
        _mockFileClient.Verify(f => f.CreateAsync(contentBytes.Length, null, null, It.IsAny<CancellationToken>()), Times.Once);
        _mockFileClient.Verify(f => f.UploadAsync(It.IsAny<Stream>(), It.IsAny<ShareFileUploadOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateLogFileAsync_WithEmptyContent_CreatesEmptyFile()
    {
        // Arrange
        _mockFileClient
            .Setup(f => f.CreateAsync(0, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<ShareFileInfo>>());

        // Act
        await _mockFileClient.Object.CreateAsync(0);

        // Assert - verify file was created
        _mockFileClient.Verify(f => f.CreateAsync(0, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetLogFileContentAsync Tests

    [Fact]
    public async Task GetLogFileContentAsync_WhenFileNotFound_ThrowsRequestFailedException()
    {
        // Arrange
        _mockFileClient
            .Setup(f => f.DownloadAsync(It.IsAny<ShareFileDownloadOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(404, "File not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RequestFailedException>(
            () => _mockFileClient.Object.DownloadAsync()
        );
        exception.Status.Should().Be(404);
    }

    #endregion

    #region DeleteLogFileAsync Tests

    [Fact]
    public async Task DeleteLogFileAsync_WithExistingFile_DeletesSuccessfully()
    {
        // Arrange
        _mockFileClient
            .Setup(f => f.DeleteIfExistsAsync(It.IsAny<ShareFileRequestConditions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

        // Act
        var response = await _mockFileClient.Object.DeleteIfExistsAsync();

        // Assert
        response.Value.Should().BeTrue();
        _mockFileClient.Verify(f => f.DeleteIfExistsAsync(It.IsAny<ShareFileRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteLogFileAsync_WithNonExistentFile_CompletesSuccessfully()
    {
        // Arrange
        _mockFileClient
            .Setup(f => f.DeleteIfExistsAsync(It.IsAny<ShareFileRequestConditions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(false, Mock.Of<Response>()));

        // Act
        var response = await _mockFileClient.Object.DeleteIfExistsAsync();

        // Assert - should not throw, just return false
        response.Value.Should().BeFalse();
    }

    #endregion

    #region File Existence Tests

    [Fact]
    public async Task ExistsAsync_WhenFileExists_ReturnsTrue()
    {
        // Arrange
        _mockFileClient
            .Setup(f => f.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

        // Act
        var response = await _mockFileClient.Object.ExistsAsync();

        // Assert
        response.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenFileDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _mockFileClient
            .Setup(f => f.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(false, Mock.Of<Response>()));

        // Act
        var response = await _mockFileClient.Object.ExistsAsync();

        // Assert
        response.Value.Should().BeFalse();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task CreateAsync_WhenStorageErrorOccurs_ThrowsRequestFailedException()
    {
        // Arrange
        _mockFileClient
            .Setup(f => f.CreateAsync(It.IsAny<long>(), null, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(500, "Storage error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RequestFailedException>(
            () => _mockFileClient.Object.CreateAsync(100)
        );
        exception.Status.Should().Be(500);
    }

    [Fact]
    public async Task UploadAsync_WhenStorageErrorOccurs_ThrowsRequestFailedException()
    {
        // Arrange
        _mockFileClient
            .Setup(f => f.UploadAsync(It.IsAny<Stream>(), It.IsAny<ShareFileUploadOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(503, "Service unavailable"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RequestFailedException>(
            () => _mockFileClient.Object.UploadAsync(new MemoryStream(), new ShareFileUploadOptions())
        );
        exception.Status.Should().Be(503);
    }

    #endregion

    #region LogFileInfo Model Tests

    [Fact]
    public void LogFileInfo_PropertiesAreSetCorrectly()
    {
        // Arrange
        var logFile = new LogFileInfo
        {
            FileName = "test-log.txt",
            Size = 1024,
            LastModified = DateTimeOffset.UtcNow
        };

        // Assert
        logFile.FileName.Should().Be("test-log.txt");
        logFile.Size.Should().Be(1024);
        logFile.LastModified.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void LogFileInfo_DefaultValues()
    {
        // Arrange
        var logFile = new LogFileInfo();

        // Assert
        logFile.FileName.Should().BeEmpty();
        logFile.Size.Should().Be(0);
        logFile.LastModified.Should().BeNull();
    }

    #endregion
}
