namespace Shipping.Entities;

public class PaymentInformation() : Entity(Guid.NewGuid())
{
    public string LastFourDigits { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    
    public decimal Amount { get; set; }
    
    public DateTime? UpdatedAtUtc { get; set; }
    
    public Guid OrderId { get; set; }
    public Order Order { get; set; } 
}