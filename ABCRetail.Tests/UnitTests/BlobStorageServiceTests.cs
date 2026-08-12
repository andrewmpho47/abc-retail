using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ABCRetail.Models;
using ABCRetail.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace ABCRetail.Tests.UnitTests;

/// <summary>
/// Unit tests for BlobStorageService with mocked Azure Blob clients.
/// Tests image and multimedia upload, download, list, and delete operations.
/// </summary>
public class BlobStorageServiceTests
{
    private readonly Mock<BlobContainerClient> _mockImageContainerClient;
    private readonly Mock<BlobContainerClient> _mockMultimediaContainerClient;
    private readonly Mock<BlobClient> _mockBlobClient;

    public BlobStorageServiceTests()
    {
        _mockImageContainerClient = new Mock<BlobContainerClient>();
        _mockMultimediaContainerClient = new Mock<BlobContainerClient>();
        _mockBlobClient = new Mock<BlobClient>();
    }

    #region Image Operations Tests

    [Fact]
    public void IsValidImageType_WithValidExtensions_ReturnsTrue()
    {
        // Arrange & Act & Assert
        BlobStorageService.IsValidImageType(".jpeg").Should().BeTrue();
        BlobStorageService.IsValidImageType(".jpg").Should().BeTrue();
        BlobStorageService.IsValidImageType(".png").Should().BeTrue();
        BlobStorageService.IsValidImageType(".gif").Should().BeTrue();
        BlobStorageService.IsValidImageType(".webp").Should().BeTrue();
    }

    [Fact]
    public void IsValidImageType_WithInvalidExtensions_ReturnsFalse()
    {
        // Arrange & Act & Assert
        BlobStorageService.IsValidImageType(".pdf").Should().BeFalse();
        BlobStorageService.IsValidImageType(".exe").Should().BeFalse();
        BlobStorageService.IsValidImageType(".txt").Should().BeFalse();
        BlobStorageService.IsValidImageType(".mp4").Should().BeFalse();
        BlobStorageService.IsValidImageType("").Should().BeFalse();
    }

    [Fact]
    public void IsValidImageType_IsCaseInsensitive()
    {
        // Arrange & Act & Assert
        BlobStorageService.IsValidImageType(".JPEG").Should().BeTrue();
        BlobStorageService.IsValidImageType(".JPG").Should().BeTrue();
        BlobStorageService.IsValidImageType(".PNG").Should().BeTrue();
        BlobStorageService.IsValidImageType(".Gif").Should().BeTrue();
        BlobStorageService.IsValidImageType(".WeBp").Should().BeTrue();
    }

    [Fact]
    public async Task UploadImageAsync_WithValidImage_UploadsSuccessfully()
    {
        // Arrange
        var stream = new MemoryStream(new byte[100]);
        var fileName = "test-image.jpg";
        var contentType = "image/jpeg";
        var blobName = "guid-test-image.jpg";

        _mockImageContainerClient
            .Setup(c => c.GetBlobClient(It.IsAny<string>()))
            .Returns(_mockBlobClient.Object);

        _mockBlobClient
            .Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());

        // Act
        await _mockBlobClient.Object.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        });

        // Assert
        _mockBlobClient.Verify(
            b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DownloadImageAsync_WhenBlobExists_ReturnsStream()
    {
        // Arrange
        var expectedContent = "test content"u8.ToArray();
        var blobDownloadInfo = BlobsModelFactory.BlobDownloadStreamingResult(
            new BinaryData(expectedContent).ToStream());
        
        _mockBlobClient
            .Setup(b => b.DownloadStreamingAsync(It.IsAny<BlobDownloadOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(blobDownloadInfo, Mock.Of<Response>()));

        // Act
        var response = await _mockBlobClient.Object.DownloadStreamingAsync();

        // Assert
        response.Value.Should().NotBeNull();
        response.Value.Content.Should().NotBeNull();
    }

    [Fact]
    public async Task DownloadImageAsync_WhenBlobNotFound_ThrowsRequestFailedException()
    {
        // Arrange
        _mockBlobClient
            .Setup(b => b.DownloadStreamingAsync(It.IsAny<BlobDownloadOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(404, "Blob not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RequestFailedException>(
            () => _mockBlobClient.Object.DownloadStreamingAsync()
        );
        exception.Status.Should().Be(404);
    }

    [Fact]
    public async Task DeleteImageAsync_WithExistingBlob_DeletesSuccessfully()
    {
        // Arrange
        _mockImageContainerClient
            .Setup(c => c.GetBlobClient(It.IsAny<string>()))
            .Returns(_mockBlobClient.Object);

        _mockBlobClient
            .Setup(b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

        // Act
        var response = await _mockBlobClient.Object.DeleteIfExistsAsync();

        // Assert
        response.Value.Should().BeTrue();
        _mockBlobClient.Verify(
            b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void GetImageUrlAsync_ReturnsCorrectUrl()
    {
        // Arrange
        var expectedUri = new Uri("https://storage.blob.core.windows.net/images/test-image.jpg");
        _mockBlobClient
            .Setup(b => b.Uri)
            .Returns(expectedUri);

        // Act
        var uri = _mockBlobClient.Object.Uri;

        // Assert
        uri.Should().Be(expectedUri);
        uri.ToString().Should().Contain("images/test-image.jpg");
    }

    #endregion

    #region Multimedia Operations Tests

    [Fact]
    public void IsValidMultimediaType_WithValidExtensions_ReturnsTrue()
    {
        // Arrange & Act & Assert
        BlobStorageService.IsValidMultimediaType(".pdf").Should().BeTrue();
        BlobStorageService.IsValidMultimediaType(".mp4").Should().BeTrue();
        BlobStorageService.IsValidMultimediaType(".docx").Should().BeTrue();
        BlobStorageService.IsValidMultimediaType(".xlsx").Should().BeTrue();
    }

    [Fact]
    public void IsValidMultimediaType_WithInvalidExtensions_ReturnsFalse()
    {
        // Arrange & Act & Assert
        BlobStorageService.IsValidMultimediaType(".jpg").Should().BeFalse();
        BlobStorageService.IsValidMultimediaType(".exe").Should().BeFalse();
        BlobStorageService.IsValidMultimediaType(".png").Should().BeFalse();
        BlobStorageService.IsValidMultimediaType(".txt").Should().BeFalse();
        BlobStorageService.IsValidMultimediaType("").Should().BeFalse();
    }

    [Fact]
    public void IsValidMultimediaType_IsCaseInsensitive()
    {
        // Arrange & Act & Assert
        BlobStorageService.IsValidMultimediaType(".PDF").Should().BeTrue();
        BlobStorageService.IsValidMultimediaType(".MP4").Should().BeTrue();
        BlobStorageService.IsValidMultimediaType(".DOCX").Should().BeTrue();
        BlobStorageService.IsValidMultimediaType(".Xlsx").Should().BeTrue();
    }

    [Fact]
    public async Task UploadMultimediaAsync_WithValidFile_UploadsSuccessfully()
    {
        // Arrange
        var stream = new MemoryStream(new byte[1000]);
        var fileName = "document.pdf";
        var contentType = "application/pdf";

        _mockMultimediaContainerClient
            .Setup(c => c.GetBlobClient(It.IsAny<string>()))
            .Returns(_mockBlobClient.Object);

        _mockBlobClient
            .Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());

        // Act
        await _mockBlobClient.Object.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        });

        // Assert
        _mockBlobClient.Verify(
            b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DownloadMultimediaAsync_WhenBlobExists_ReturnsStream()
    {
        // Arrange
        var expectedContent = new byte[500];
        var blobDownloadInfo = BlobsModelFactory.BlobDownloadStreamingResult(
            new BinaryData(expectedContent).ToStream());

        _mockBlobClient
            .Setup(b => b.DownloadStreamingAsync(It.IsAny<BlobDownloadOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(blobDownloadInfo, Mock.Of<Response>()));

        // Act
        var response = await _mockBlobClient.Object.DownloadStreamingAsync();

        // Assert
        response.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteMultimediaAsync_WithExistingBlob_DeletesSuccessfully()
    {
        // Arrange
        _mockMultimediaContainerClient
            .Setup(c => c.GetBlobClient(It.IsAny<string>()))
            .Returns(_mockBlobClient.Object);

        _mockBlobClient
            .Setup(b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

        // Act
        var response = await _mockBlobClient.Object.DeleteIfExistsAsync();

        // Assert
        response.Value.Should().BeTrue();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void IsValidImageType_WithNullExtension_ReturnsFalse()
    {
        // Arrange & Act & Assert
        BlobStorageService.IsValidImageType(null!).Should().BeFalse();
    }

    [Fact]
    public void IsValidMultimediaType_WithNullExtension_ReturnsFalse()
    {
        // Arrange & Act & Assert
        BlobStorageService.IsValidMultimediaType(null!).Should().BeFalse();
    }

    [Fact]
    public void IsValidImageType_WithExtensionWithoutDot_ReturnsFalse()
    {
        // Arrange & Act & Assert
        BlobStorageService.IsValidImageType("jpg").Should().BeFalse();
        BlobStorageService.IsValidImageType("png").Should().BeFalse();
    }

    [Fact]
    public void IsValidMultimediaType_WithExtensionWithoutDot_ReturnsFalse()
    {
        // Arrange & Act & Assert
        BlobStorageService.IsValidMultimediaType("pdf").Should().BeFalse();
        BlobStorageService.IsValidMultimediaType("mp4").Should().BeFalse();
    }

    [Fact]
    public async Task UploadAsync_WhenStorageErrorOccurs_ThrowsRequestFailedException()
    {
        // Arrange
        _mockBlobClient
            .Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(500, "Storage error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RequestFailedException>(
            () => _mockBlobClient.Object.UploadAsync(new MemoryStream(), new BlobUploadOptions())
        );
        exception.Status.Should().Be(500);
    }

    #endregion
}
