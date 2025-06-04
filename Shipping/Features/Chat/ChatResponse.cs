namespace Shipping.Features.Chat;

public record ChatResponse(int Id,
    string RecipientName,
    DateTime LastMessageAtUtc,
    List<ChatMessageResponse>? Messages = null);