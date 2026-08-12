using ABCRetail.Models;
using ABCRetail.Models.ViewModels;
using FsCheck;

namespace ABCRetail.Tests.Generators;

/// <summary>
/// FsCheck Arbitrary implementations for domain objects.
/// Used to generate random test data for property-based tests.
/// </summary>
public static class DomainArbitraries
{
    /// <summary>
    /// Generates non-empty alphanumeric strings suitable for keys.
    /// </summary>
    public static Gen<string> NonEmptyAlphanumericString =>
        Gen.Elements(
            "ABC", "DEF", "GHI", "JKL", "MNO", "PQR", "STU", "VWX", "YZ1", "234",
            "Order001", "Cust001", "Prod001", "Region1", "Category1", "SKU123",
            "User123", "Item456", "Key789", "Test001", "Sample01", "Data001"
        );

    /// <summary>
    /// Generates random positive integers for quantities.
    /// </summary>
    public static Gen<int> PositiveQuantity =>
        Gen.Choose(1, 10000);

    /// <summary>
    /// Generates random action types for inventory messages.
    /// </summary>
    public static Gen<string> ActionType =>
        Gen.Elements("Restock", "Deduct", "Alert");

    /// <summary>
    /// Generates random reason strings.
    /// </summary>
    public static Gen<string> ReasonString =>
        Gen.Elements(
            "Weekly restock", "Customer order", "Low stock alert", "Seasonal adjustment",
            "Return processing", "Inventory audit", "Promotional event", "Supplier delivery",
            "Damaged goods", "Quality control"
        );

    /// <summary>
    /// Generates random order status values.
    /// </summary>
    public static Gen<string> OrderStatus =>
        Gen.Elements("Pending", "Processing", "Shipped", "Delivered", "Cancelled");

    /// <summary>
    /// Generates random first names.
    /// </summary>
    public static Gen<string> FirstName =>
        Gen.Elements(
            "John", "Jane", "Michael", "Sarah", "David", "Emily",
            "Chris", "Amanda", "James", "Lisa", "Robert", "Jennifer"
        );

    /// <summary>
    /// Generates random last names.
    /// </summary>
    public static Gen<string> LastName =>
        Gen.Elements(
            "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia",
            "Miller", "Davis", "Wilson", "Taylor", "Anderson", "Thomas"
        );

    /// <summary>
    /// Generates random valid email addresses.
    /// </summary>
    public static Gen<string> ValidEmail =>
        Gen.Elements(
            "test@example.com", "user@domain.org", "admin@company.net",
            "support@service.io", "info@business.com", "contact@site.org"
        );

    /// <summary>
    /// Generates random phone numbers.
    /// </summary>
    public static Gen<string> PhoneNumber =>
        Gen.Elements(
            "555-0100", "555-0101", "555-0102", "555-0103", "555-0104",
            "123-456-7890", "987-654-3210", "(555) 123-4567"
        );

    /// <summary>
    /// Generates random address strings.
    /// </summary>
    public static Gen<string> Address =>
        Gen.Elements(
            "123 Main St, City, ST 12345", "456 Oak Ave, Town, ST 67890",
            "789 Pine Rd, Village, ST 11111", "321 Elm Blvd, County, ST 22222"
        );

    /// <summary>
    /// Generates random image names.
    /// </summary>
    public static Gen<string> ImageName =>
        Gen.Elements(
            "product-image.jpg", "banner.png", "logo.gif", "photo.webp",
            "thumbnail.jpeg", "hero-image.png", "gallery-01.jpg", "icon.gif"
        );
}

/// <summary>
/// Arbitrary for OrderMessage generation.
/// </summary>
public class OrderMessageArbitrary
{
    public static Arbitrary<OrderMessage> Arbitrary =>
        (from orderId in DomainArbitraries.NonEmptyAlphanumericString
         from customerId in DomainArbitraries.NonEmptyAlphanumericString
         from productId in DomainArbitraries.NonEmptyAlphanumericString
         from quantity in DomainArbitraries.PositiveQuantity
         from status in DomainArbitraries.OrderStatus
         select new OrderMessage
         {
             OrderId = orderId,
             CustomerId = customerId,
             ProductId = productId,
             Quantity = quantity,
             OrderStatus = status
         }).ToArbitrary();
}

/// <summary>
/// Arbitrary for InventoryMessage generation.
/// </summary>
public class InventoryMessageArbitrary
{
    public static Arbitrary<InventoryMessage> Arbitrary =>
        (from productId in DomainArbitraries.NonEmptyAlphanumericString
         from actionType in DomainArbitraries.ActionType
         from quantity in DomainArbitraries.PositiveQuantity
         from reason in DomainArbitraries.ReasonString
         select new InventoryMessage
         {
             ProductId = productId,
             ActionType = actionType,
             Quantity = quantity,
             Reason = reason
         }).ToArbitrary();
}

/// <summary>
/// Arbitrary for CustomerFormViewModel generation (valid data).
/// </summary>
public class ValidCustomerFormViewModelArbitrary
{
    public static Arbitrary<CustomerFormViewModel> Arbitrary =>
        (from partitionKey in DomainArbitraries.NonEmptyAlphanumericString
         from rowKey in DomainArbitraries.NonEmptyAlphanumericString
         from firstName in DomainArbitraries.FirstName
         from lastName in DomainArbitraries.LastName
         from email in DomainArbitraries.ValidEmail
         from phoneNumber in DomainArbitraries.PhoneNumber
         from address in DomainArbitraries.Address
         select new CustomerFormViewModel
         {
             PartitionKey = partitionKey,
             RowKey = rowKey,
             FirstName = firstName,
             LastName = lastName,
             Email = email,
             PhoneNumber = phoneNumber,
             Address = address
         }).ToArbitrary();
}

/// <summary>
/// Arbitrary for CustomerFormViewModel generation with at least one invalid required field.
/// </summary>
public class InvalidCustomerFormViewModelArbitrary
{
    public static Arbitrary<CustomerFormViewModel> Arbitrary =>
        (from invalidFieldChoice in Gen.Choose(0, 4)
         from partitionKey in DomainArbitraries.NonEmptyAlphanumericString
         from rowKey in DomainArbitraries.NonEmptyAlphanumericString
         from firstName in DomainArbitraries.FirstName
         from lastName in DomainArbitraries.LastName
         from email in DomainArbitraries.ValidEmail
         select new CustomerFormViewModel
         {
             // Make one required field empty based on the choice
             PartitionKey = invalidFieldChoice == 0 ? string.Empty : partitionKey,
             RowKey = invalidFieldChoice == 1 ? string.Empty : rowKey,
             FirstName = invalidFieldChoice == 2 ? string.Empty : firstName,
             LastName = invalidFieldChoice == 3 ? string.Empty : lastName,
             Email = invalidFieldChoice == 4 ? string.Empty : email,
             PhoneNumber = string.Empty, // Optional field
             Address = string.Empty // Optional field
         }).ToArbitrary();
}

/// <summary>
/// Arbitrary for image name generation (for notification tests).
/// </summary>
public class ImageNameArbitrary
{
    public static Arbitrary<string> Arbitrary =>
        DomainArbitraries.ImageName.ToArbitrary();
}
