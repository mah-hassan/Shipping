namespace Shipping.Entities;

public class Complaint() : Entity
{
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public Company AgainstCompany { get; set; }
    public int AgainstCompanyId { get; set; }
    public int OrderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public ComplaintType Type { get; set; }
    public ComplaintStatus Status { get; set; }
}