using Shipping.Features.Chat;

namespace Shipping.Hubs.ChatHub;

public interface IChatClient
{
    Task ReceiveMessage(ChatMessageResponse message);
}