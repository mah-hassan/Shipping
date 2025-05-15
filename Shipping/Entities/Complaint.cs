namespace Shipping.Entities;

public class Complaint() : Entity(Guid.NewGuid())
{
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public Company AgainstCompany { get; set; }
    public Guid AgainstCompanyId { get; set; }
    public Guid OrderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public ComplaintType Type { get; set; }
    public ComplaintStatus Status { get; set; }
}