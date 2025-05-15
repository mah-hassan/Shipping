namespace Shipping.Entities;

public class Review() : Entity(Guid.NewGuid())
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid OrderId { get; set; }
    public Guid CompanyId { get; set; }
    
    public string? Comment { get; set; } = string.Empty;
    public int Rating { get; set; }
    
    public bool PackageDamaged { get; set; }
    public bool DeliveryLate { get; set; }
}