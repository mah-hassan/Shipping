namespace Shipping.Entities;

public class Message : Entity
{
    public Message() {}

    public int ChatId { get; set; }
    public Chat Chat { get; set; }

    public int SenderId { get; set; }
    public User Sender { get; set; }

    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
}