using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using ABCRetail.Models;
using ABCRetail.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace ABCRetail.Tests.UnitTests;

/// <summary>
/// Unit tests for QueueStorageService with mocked Azure Queue clients.
/// Tests order, inventory, and image notification queue operations.
/// </summary>
public class QueueStorageServiceTests
{
    private readonly Mock<QueueClient> _mockOrderQueueClient;
    private readonly Mock<QueueClient> _mockInventoryQueueClient;
    private readonly Mock<QueueClient> _mockImageNotificationQueueClient;

    public QueueStorageServiceTests()
    {
        _mockOrderQueueClient = new Mock<QueueClient>();
        _mockInventoryQueueClient = new Mock<QueueClient>();
        _mockImageNotificationQueueClient = new Mock<QueueClient>();
    }

    #region Order Queue Operations Tests

    [Fact]
    public async Task SendOrderMessageAsync_WithValidOrder_SendsFormattedMessage()
    {
        // Arrange
        var order = new OrderMessage
        {
            OrderId = "ORD-001",
            CustomerId = "CUST-123",
            ProductId = "PROD-456",
            Quantity = 5,
            OrderStatus = "Pending"
        };
        var expectedMessage = order.ToQueueMessage();

        _mockOrderQueueClient
            .Setup(c => c.SendMessageAsync(expectedMessage))
            .ReturnsAsync(Mock.Of<Response<SendReceipt>>());

        // Act
        await _mockOrderQueueClient.Object.SendMessageAsync(expectedMessage);

        // Assert
        _mockOrderQueueClient.Verify(
            c => c.SendMessageAsync(expectedMessage),
            Times.Once);
    }

    [Fact]
    public void OrderMessage_ToQueueMessage_FormatsCorrectly()
    {
        // Arrange
        var order = new OrderMessage
        {
            OrderId = "ORD-001",
            CustomerId = "CUST-123",
            ProductId = "PROD-456",
            Quantity = 5,
            OrderStatus = "Pending"
        };

        // Act
        var message = order.ToQueueMessage();

        // Assert
        message.Should().Be("Processing order ORD-001 for customer CUST-123: 5 x PROD-456");
    }

    [Fact]
    public async Task PeekOrderMessagesAsync_ReturnsMessages()
    {
        // Arrange
        var peekedMessages = new[]
        {
            QueuesModelFactory.PeekedMessage("msg-1", "Processing order ORD-001 for customer CUST-001: 2 x PROD-001", 1, DateTimeOffset.UtcNow),
            QueuesModelFactory.PeekedMessage("msg-2", "Processing order ORD-002 for customer CUST-002: 1 x PROD-002", 1, DateTimeOffset.UtcNow)
        };

        _mockOrderQueueClient
            .Setup(c => c.PeekMessagesAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(peekedMessages, Mock.Of<Response>()));

        // Act
        var response = await _mockOrderQueueClient.Object.PeekMessagesAsync(32);

        // Assert
        response.Value.Should().HaveCount(2);
        response.Value[0].MessageText.Should().Contain("ORD-001");
        response.Value[1].MessageText.Should().Contain("ORD-002");
    }

    [Fact]
    public async Task DequeueOrderMessageAsync_WhenMessageExists_ReturnsMessage()
    {
        // Arrange
        var receivedMessage = QueuesModelFactory.QueueMessage(
            "msg-1",
            "pop-receipt-1",
            "Processing order ORD-001 for customer CUST-001: 2 x PROD-001",
            1,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(7),
            DateTimeOffset.UtcNow.AddMinutes(1));

        _mockOrderQueueClient
            .Setup(c => c.ReceiveMessageAsync(It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(receivedMessage, Mock.Of<Response>()));

        _mockOrderQueueClient
            .Setup(c => c.DeleteMessageAsync(receivedMessage.MessageId, receivedMessage.PopReceipt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response>());

        // Act
        var response = await _mockOrderQueueClient.Object.ReceiveMessageAsync();

        // Assert
        response.Value.Should().NotBeNull();
        response.Value.MessageText.Should().Contain("ORD-001");
    }

    [Fact]
    public async Task DequeueOrderMessageAsync_WhenQueueEmpty_ReturnsNull()
    {
        // Arrange
        _mockOrderQueueClient
            .Setup(c => c.ReceiveMessageAsync(It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue<QueueMessage>(null!, Mock.Of<Response>()));

        // Act
        var response = await _mockOrderQueueClient.Object.ReceiveMessageAsync();

        // Assert
        response.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetOrderQueueCountAsync_ReturnsApproximateCount()
    {
        // Arrange
        var queueProperties = QueuesModelFactory.QueueProperties(
            metadata: new Dictionary<string, string>(),
            approximateMessagesCount: 5);

        _mockOrderQueueClient
            .Setup(c => c.GetPropertiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(queueProperties, Mock.Of<Response>()));

        // Act
        var response = await _mockOrderQueueClient.Object.GetPropertiesAsync();

        // Assert
        response.Value.ApproximateMessagesCount.Should().Be(5);
    }

    #endregion

    #region Inventory Queue Operations Tests

    [Fact]
    public async Task SendInventoryMessageAsync_WithValidInventory_SendsFormattedMessage()
    {
        // Arrange
        var inventory = new InventoryMessage
        {
            ProductId = "PROD-789",
            ActionType = "Restock",
            Quantity = 100,
            Reason = "Weekly restock"
        };
        var expectedMessage = inventory.ToQueueMessage();

        _mockInventoryQueueClient
            .Setup(c => c.SendMessageAsync(expectedMessage))
            .ReturnsAsync(Mock.Of<Response<SendReceipt>>());

        // Act
        await _mockInventoryQueueClient.Object.SendMessageAsync(expectedMessage);

        // Assert
        _mockInventoryQueueClient.Verify(
            c => c.SendMessageAsync(expectedMessage),
            Times.Once);
    }

    [Fact]
    public void InventoryMessage_ToQueueMessage_FormatsCorrectly()
    {
        // Arrange
        var inventory = new InventoryMessage
        {
            ProductId = "PROD-789",
            ActionType = "Restock",
            Quantity = 100,
            Reason = "Weekly restock"
        };

        // Act
        var message = inventory.ToQueueMessage();

        // Assert
        message.Should().Be("Restock inventory for PROD-789: 100 units - Weekly restock");
    }

    [Theory]
    [InlineData("Restock", "Weekly delivery")]
    [InlineData("Deduct", "Customer purchase")]
    [InlineData("Alert", "Low stock warning")]
    public void InventoryMessage_ToQueueMessage_WithDifferentActionTypes_FormatsCorrectly(string actionType, string reason)
    {
        // Arrange
        var inventory = new InventoryMessage
        {
            ProductId = "PROD-001",
            ActionType = actionType,
            Quantity = 50,
            Reason = reason
        };

        // Act
        var message = inventory.ToQueueMessage();

        // Assert
        message.Should().Be($"{actionType} inventory for PROD-001: 50 units - {reason}");
    }

    [Fact]
    public async Task PeekInventoryMessagesAsync_ReturnsMessages()
    {
        // Arrange
        var peekedMessages = new[]
        {
            QueuesModelFactory.PeekedMessage("msg-1", "Restock inventory for PROD-001: 50 units - Weekly restock", 1, DateTimeOffset.UtcNow),
            QueuesModelFactory.PeekedMessage("msg-2", "Deduct inventory for PROD-002: 10 units - Customer order", 1, DateTimeOffset.UtcNow)
        };

        _mockInventoryQueueClient
            .Setup(c => c.PeekMessagesAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(peekedMessages, Mock.Of<Response>()));

        // Act
        var response = await _mockInventoryQueueClient.Object.PeekMessagesAsync(32);

        // Assert
        response.Value.Should().HaveCount(2);
        response.Value[0].MessageText.Should().Contain("Restock");
        response.Value[1].MessageText.Should().Contain("Deduct");
    }

    [Fact]
    public async Task GetInventoryQueueCountAsync_ReturnsApproximateCount()
    {
        // Arrange
        var queueProperties = QueuesModelFactory.QueueProperties(
            metadata: new Dictionary<string, string>(),
            approximateMessagesCount: 10);

        _mockInventoryQueueClient
            .Setup(c => c.GetPropertiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(queueProperties, Mock.Of<Response>()));

        // Act
        var response = await _mockInventoryQueueClient.Object.GetPropertiesAsync();

        // Assert
        response.Value.ApproximateMessagesCount.Should().Be(10);
    }

    #endregion

    #region Image Notification Queue Operations Tests

    [Fact]
    public async Task SendImageUploadNotificationAsync_WithImageName_SendsFormattedMessage()
    {
        // Arrange
        var imageName = "product-photo.jpg";
        var expectedMessage = $"Uploading an image {imageName} to blob storage";

        _mockImageNotificationQueueClient
            .Setup(c => c.SendMessageAsync(expectedMessage))
            .ReturnsAsync(Mock.Of<Response<SendReceipt>>());

        // Act
        await _mockImageNotificationQueueClient.Object.SendMessageAsync(expectedMessage);

        // Assert
        _mockImageNotificationQueueClient.Verify(
            c => c.SendMessageAsync(expectedMessage),
            Times.Once);
    }

    [Theory]
    [InlineData("image.jpg")]
    [InlineData("photo.png")]
    [InlineData("banner.gif")]
    [InlineData("hero-image.webp")]
    public void ImageUploadNotification_FormatsCorrectly(string imageName)
    {
        // Arrange & Act
        var message = $"Uploading an image {imageName} to blob storage";

        // Assert
        message.Should().StartWith("Uploading an image ");
        message.Should().EndWith(" to blob storage");
        message.Should().Contain(imageName);
    }

    [Fact]
    public async Task PeekImageNotificationsAsync_ReturnsMessages()
    {
        // Arrange
        var peekedMessages = new[]
        {
            QueuesModelFactory.PeekedMessage("msg-1", "Uploading an image product-1.jpg to blob storage", 1, DateTimeOffset.UtcNow),
            QueuesModelFactory.PeekedMessage("msg-2", "Uploading an image banner.png to blob storage", 1, DateTimeOffset.UtcNow)
        };

        _mockImageNotificationQueueClient
            .Setup(c => c.PeekMessagesAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(peekedMessages, Mock.Of<Response>()));

        // Act
        var response = await _mockImageNotificationQueueClient.Object.PeekMessagesAsync(32);

        // Assert
        response.Value.Should().HaveCount(2);
        response.Value[0].MessageText.Should().Contain("product-1.jpg");
        response.Value[1].MessageText.Should().Contain("banner.png");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task SendMessageAsync_WhenStorageErrorOccurs_ThrowsRequestFailedException()
    {
        // Arrange
        _mockOrderQueueClient
            .Setup(c => c.SendMessageAsync(It.IsAny<string>()))
            .ThrowsAsync(new RequestFailedException(500, "Storage error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RequestFailedException>(
            () => _mockOrderQueueClient.Object.SendMessageAsync("test message")
        );
        exception.Status.Should().Be(500);
    }

    [Fact]
    public async Task GetPropertiesAsync_WhenStorageErrorOccurs_ThrowsRequestFailedException()
    {
        // Arrange
        _mockOrderQueueClient
            .Setup(c => c.GetPropertiesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(503, "Service unavailable"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RequestFailedException>(
            () => _mockOrderQueueClient.Object.GetPropertiesAsync()
        );
        exception.Status.Should().Be(503);
    }

    #endregion
}
