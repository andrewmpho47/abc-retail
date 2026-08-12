namespace ABCRetail.Models;

/// <summary>
/// Represents information about a message in Azure Queue Storage.
/// Used for displaying queue message details in the UI.
/// </summary>
public class QueueMessageInfo
{
    /// <summary>
    /// Unique identifier of the message in the queue.
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// The text content of the message.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when the message was inserted into the queue.
    /// </summary>
    public DateTimeOffset? InsertedOn { get; set; }

    /// <summary>
    /// The timestamp when the message will expire and be automatically deleted.
    /// </summary>
    public DateTimeOffset? ExpiresOn { get; set; }

    /// <summary>
    /// The pop receipt required to delete or update the message after dequeuing.
    /// </summary>
    public string PopReceipt { get; set; } = string.Empty;
}
