namespace Shipping.Entities;

public class Notification() : Entity(Guid.NewGuid())
{
    public string Content { get; set; } = string.Empty;
    public Guid ReceiverId { get; set; }
    public NotificationType Type { get; set; }
}