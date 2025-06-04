namespace Shipping.Entities;

public class Chat : Entity
{
    public Chat() {}

    public int ParticipantOneId { get; set; }
    public User ParticipantOne { get; set; }

    public int ParticipantTwoId { get; set; }
    public User ParticipantTwo { get; set; }
    public DateTime LastMessageAtUtc { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();

    public void AddMessage(Message message)
    {
        Messages.Add(message);
        LastMessageAtUtc = DateTime.UtcNow;
    }
}