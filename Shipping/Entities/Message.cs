namespace Shipping.Entities;

public class Message : Entity
{
    public Message() : base(Guid.NewGuid()) {}

    public Guid ChatId { get; set; }
    public Chat Chat { get; set; }

    public Guid SenderId { get; set; }
    public User Sender { get; set; }

    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
}