namespace Shipping.Features.Chat;

public class ChatMessageResponse
{
    public int ChatId { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; }
}