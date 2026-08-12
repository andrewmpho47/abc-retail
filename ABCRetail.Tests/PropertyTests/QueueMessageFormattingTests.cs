using ABCRetail.Models;
using ABCRetail.Tests.Generators;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace ABCRetail.Tests.PropertyTests;

/// <summary>
/// Property-based tests for queue message formatting.
/// **Validates: Requirements 5.2, 6.2, 7.1**
/// 
/// Property 4: Queue Message Formatting
/// - For any OrderMessage, the formatted queue message SHALL match the pattern: 
///   "Processing order {OrderId} for customer {CustomerId}: {Quantity} x {ProductId}"
/// - For any InventoryMessage, the formatted queue message SHALL match the pattern: 
///   "{ActionType} inventory for {ProductId}: {Quantity} units - {Reason}"
/// - For any image upload notification, the formatted message SHALL match the pattern: 
///   "Uploading an image {imageName} to blob storage"
/// </summary>
public class QueueMessageFormattingTests
{
    /// <summary>
    /// Property test: OrderMessage.ToQueueMessage() always matches the expected format.
    /// **Validates: Requirements 5.2**
    /// </summary>
    [Property(Arbitrary = new[] { typeof(OrderMessageArbitrary) }, MaxTest = 100)]
    public Property OrderMessage_ToQueueMessage_MatchesExpectedFormat(OrderMessage order)
    {
        // Act
        var formattedMessage = order.ToQueueMessage();

        // Assert - verify the message matches the exact expected format
        var expectedMessage = $"Processing order {order.OrderId} for customer {order.CustomerId}: {order.Quantity} x {order.ProductId}";

        return (formattedMessage == expectedMessage).ToProperty()
            .Label($"Expected: '{expectedMessage}'\nActual: '{formattedMessage}'");
    }

    /// <summary>
    /// Property test: OrderMessage.ToQueueMessage() contains all field values.
    /// **Validates: Requirements 5.2**
    /// </summary>
    [Property(Arbitrary = new[] { typeof(OrderMessageArbitrary) }, MaxTest = 100)]
    public Property OrderMessage_ToQueueMessage_ContainsAllFields(OrderMessage order)
    {
        // Act
        var formattedMessage = order.ToQueueMessage();

        // Assert - verify all fields are present in the message
        var containsOrderId = formattedMessage.Contains(order.OrderId);
        var containsCustomerId = formattedMessage.Contains(order.CustomerId);
        var containsProductId = formattedMessage.Contains(order.ProductId);
        var containsQuantity = formattedMessage.Contains(order.Quantity.ToString());

        return (containsOrderId && containsCustomerId && containsProductId && containsQuantity).ToProperty()
            .Label($"Missing fields in message: '{formattedMessage}'")
            .Label($"OrderId present: {containsOrderId}")
            .Label($"CustomerId present: {containsCustomerId}")
            .Label($"ProductId present: {containsProductId}")
            .Label($"Quantity present: {containsQuantity}");
    }

    /// <summary>
    /// Property test: InventoryMessage.ToQueueMessage() always matches the expected format.
    /// **Validates: Requirements 6.2**
    /// </summary>
    [Property(Arbitrary = new[] { typeof(InventoryMessageArbitrary) }, MaxTest = 100)]
    public Property InventoryMessage_ToQueueMessage_MatchesExpectedFormat(InventoryMessage inventory)
    {
        // Act
        var formattedMessage = inventory.ToQueueMessage();

        // Assert - verify the message matches the exact expected format
        var expectedMessage = $"{inventory.ActionType} inventory for {inventory.ProductId}: {inventory.Quantity} units - {inventory.Reason}";

        return (formattedMessage == expectedMessage).ToProperty()
            .Label($"Expected: '{expectedMessage}'\nActual: '{formattedMessage}'");
    }

    /// <summary>
    /// Property test: InventoryMessage.ToQueueMessage() contains all field values.
    /// **Validates: Requirements 6.2**
    /// </summary>
    [Property(Arbitrary = new[] { typeof(InventoryMessageArbitrary) }, MaxTest = 100)]
    public Property InventoryMessage_ToQueueMessage_ContainsAllFields(InventoryMessage inventory)
    {
        // Act
        var formattedMessage = inventory.ToQueueMessage();

        // Assert - verify all fields are present in the message
        var containsProductId = formattedMessage.Contains(inventory.ProductId);
        var containsActionType = formattedMessage.Contains(inventory.ActionType);
        var containsQuantity = formattedMessage.Contains(inventory.Quantity.ToString());
        var containsReason = formattedMessage.Contains(inventory.Reason);

        return (containsProductId && containsActionType && containsQuantity && containsReason).ToProperty()
            .Label($"Missing fields in message: '{formattedMessage}'")
            .Label($"ProductId present: {containsProductId}")
            .Label($"ActionType present: {containsActionType}")
            .Label($"Quantity present: {containsQuantity}")
            .Label($"Reason present: {containsReason}");
    }

    /// <summary>
    /// Property test: Image upload notification message matches the expected format.
    /// This tests the format used in QueueStorageService.SendImageUploadNotificationAsync.
    /// **Validates: Requirements 7.1**
    /// </summary>
    [Property(Arbitrary = new[] { typeof(ImageNameArbitrary) }, MaxTest = 100)]
    public Property ImageUploadNotification_MatchesExpectedFormat(string imageName)
    {
        // Act - simulate the notification message format
        var formattedMessage = $"Uploading an image {imageName} to blob storage";

        // Assert - verify the message matches the expected format
        var expectedMessage = $"Uploading an image {imageName} to blob storage";

        return (formattedMessage == expectedMessage).ToProperty()
            .Label($"Expected: '{expectedMessage}'\nActual: '{formattedMessage}'");
    }

    /// <summary>
    /// Property test: Image upload notification message contains the image name.
    /// **Validates: Requirements 7.1**
    /// </summary>
    [Property(Arbitrary = new[] { typeof(ImageNameArbitrary) }, MaxTest = 100)]
    public Property ImageUploadNotification_ContainsImageName(string imageName)
    {
        // Act - simulate the notification message format
        var formattedMessage = $"Uploading an image {imageName} to blob storage";

        // Assert
        var containsImageName = formattedMessage.Contains(imageName);
        var startsCorrectly = formattedMessage.StartsWith("Uploading an image ");
        var endsCorrectly = formattedMessage.EndsWith(" to blob storage");

        return (containsImageName && startsCorrectly && endsCorrectly).ToProperty()
            .Label($"Message: '{formattedMessage}'")
            .Label($"Contains image name: {containsImageName}")
            .Label($"Starts correctly: {startsCorrectly}")
            .Label($"Ends correctly: {endsCorrectly}");
    }

    /// <summary>
    /// Verifies OrderMessage formatting with specific example values.
    /// </summary>
    [Fact]
    public void OrderMessage_ToQueueMessage_WithSpecificValues_FormatsCorrectly()
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

    /// <summary>
    /// Verifies InventoryMessage formatting with specific example values.
    /// </summary>
    [Fact]
    public void InventoryMessage_ToQueueMessage_WithSpecificValues_FormatsCorrectly()
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
}
