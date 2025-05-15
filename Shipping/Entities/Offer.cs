namespace Shipping.Entities;

public class Offer : Entity
{
    public Offer() : base(Guid.NewGuid())
    {
        
    }
    
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
    
    public Guid CompanyId { get; set; }
    public Company Company { get; set; }
    // offer details
    public decimal Price { get; set; }
    public int EstimatedDeliveryTimeInDays { get; set; }
    public string? Notes { get; set; }
    
    public DateTime? DeliveryDateUtc { get; set; }
    public OfferStatus Status { get; set; }
}