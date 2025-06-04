namespace Shipping.Entities;

public class Notification() : Entity
{
    public string Content { get; set; } = string.Empty;
    public int ReceiverId { get; set; }
    public required NotificationType Type { get; set; }
}