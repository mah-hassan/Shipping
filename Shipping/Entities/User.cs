namespace Shipping.Entities;
public class User : Entity
{
    public User() : base(Guid.NewGuid())
    {
    }
    
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}