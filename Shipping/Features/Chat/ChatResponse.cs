namespace Shipping.Features.Chat;

public record ChatResponse(Guid Id,
    string RecipientName,
    DateTime LastMessageAtUtc,
    List<ChatMessageResponse>? Messages = null);