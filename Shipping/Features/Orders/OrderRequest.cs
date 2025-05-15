namespace Shipping.Features.Orders;

public record OrderRequest(string PickupLocation,
    string Destination,
    int WeightInKg,
    string PackageSize,
    string? Details);