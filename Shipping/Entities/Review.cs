namespace Shipping.Entities;

public class Review() : Entity
{
    public int UserId { get; set; }
    public User User { get; set; }
    public int OrderId { get; set; }
    public int CompanyId { get; set; }
    
    public string? Comment { get; set; } = string.Empty;
    public int Rating { get; set; }
    
    public bool PackageDamaged { get; set; }
    public bool DeliveryLate { get; set; }
}