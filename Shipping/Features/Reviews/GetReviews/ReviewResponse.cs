namespace Shipping.Features.Reviews.GetReviews;

public class ReviewResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Comment { get; set; } = string.Empty;
    public int Rating { get; set; }
    public bool PackageDamaged { get; set; }
    public bool DeliveryLate { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}