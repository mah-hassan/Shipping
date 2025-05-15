namespace Shipping.Features.Reviews.AddReview;

public class ReviewRequest
{
    public Guid OrderId { get; set; }
    
    public string? Comment { get; set; } = string.Empty;
    public int Rating { get; set; }
    
    public bool PackageDamaged { get; set; }
    public bool DeliveryLate { get; set; }
}