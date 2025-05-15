namespace Shipping.Entities;

public class Order : Entity
{
    public Order() : base(Guid.NewGuid())
    {
        
    }
    
    // order information
    public string PickupLocation { get; set; }
    public string Destination { get; set; }
    public int WeightInKg { get; set; }
    public OrderStatus Status { get; set; }
    public PackageSize PackageSize { get; set; }
    public string? Details { get; set; }
    
    // owner information
    public Guid OwnerId { get; set; } 
    public User Owner { get; set; } 
    // company information
    public Guid? CompanyId { get; set; } 
    public Company? Company { get; set; } 
    // offers
    public List<Offer> Offers { get; set; } = new();
}