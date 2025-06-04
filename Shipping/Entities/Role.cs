namespace Shipping.Entities;

public class Role : Entity
{
    public Role() 
    {
    }
    public required string Name { get; init; }
    public ICollection<User> Users { get; set; }
}
