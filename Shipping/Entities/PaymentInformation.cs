namespace Shipping.Entities;

public class PaymentInformation() : Entity
{
    public string LastFourDigits { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    
    public decimal Amount { get; set; }
    
    public DateTime? UpdatedAtUtc { get; set; }
    
    public int OrderId { get; set; }
    public Order Order { get; set; } 
}