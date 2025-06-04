namespace Shipping.Features.Offers.GetOffers;

public record OfferResponse(
    int Id,
    int OrderId,
    string CustomerName,
    int CompanyId,
    string CompanyName,
    decimal Price,
    int EstimatedDeliveryTimeInDays,
    string? Notes,
    string Status,
    DateTime CreatedAtUtc,
    DateTime DeliveryDateUtc
);