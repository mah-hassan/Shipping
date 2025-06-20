namespace Shipping.Features.Orders.GetOrders;

public record OrderResponse(
    int Id,
    string PickupLocation,
    string Destination,
    int WeightInKg,
    string PackageSize,
    string OwnerName,
    string? CompanyName,
    string Status,
    string? Details,
    DateTime CreatedAtUtc,
    DateTime? DeliveredAtUtc,
    decimal Price = 0.0m
);