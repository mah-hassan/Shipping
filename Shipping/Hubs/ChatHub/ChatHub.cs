using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shipping.Features.Chat;

namespace Shipping.Hubs.ChatHub;

[Authorize] 
public class ChatHub(ShippingDbContext dbContext) : Hub<IChatClient>
{
    private readonly HashSet<int> connectedUsers = new();
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.GetUserId(); 
        if (userId == null)
        {
            Context.Abort();
            return;
        }
        
        var chatIds = await dbContext.Chats
            .Where(c => c.ParticipantOneId == userId || c.ParticipantTwoId == userId)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (var chatId in chatIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(chatId));
        }
        
        connectedUsers.Add(userId.Value);

        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.GetUserId();
        if (userId == null)
        {
            return;
        }
        
        var chatIds = await dbContext.Chats
            .Where(c => c.ParticipantOneId == userId || c.ParticipantTwoId == userId)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (var chatId in chatIds)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(chatId));
        }

        connectedUsers.Remove(userId.Value);
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(int chatId, string content)
    {
        var senderId = Context.User!.GetUserId();

        var chat = await dbContext.Chats
            .Include(c => c.ParticipantOne)
            .Include(c => c.ParticipantTwo)
            .FirstOrDefaultAsync(c => c.Id == chatId);

        if (chat == null || (chat.ParticipantOneId != senderId && chat.ParticipantTwoId != senderId))
        {
            throw new HubException("Unauthorized or invalid chat");
        }

        (User sender, User receiver) = chat.ParticipantOneId == senderId ?
            (chat.ParticipantOne, chat.ParticipantTwo) : (chat.ParticipantTwo, chat.ParticipantOne);
        
        
        var message = new Message
        {
            ChatId = chatId,
            SenderId = senderId,
            Content = content
        };

        dbContext.Messages.Add(message);
        
        chat.LastMessageAtUtc = message.CreatedAtUtc;
        dbContext.Chats.Update(chat);

        if (!connectedUsers.Contains(receiver.Id))
        {
            var notification = new Notification
            {
                Content = "New message from " + sender.FullName,
                ReceiverId = receiver.Id,
                Type = NotificationType.MessageReceived
            };
            
            dbContext.Notifications.Add(notification);
        }
        
        await dbContext.SaveChangesAsync();

        var dto = new ChatMessageResponse
        {
            ChatId = chatId,
            SenderId = senderId,
            SenderName = sender.FullName,
            Content = content,
            SentAtUtc = message.CreatedAtUtc
        };

        await Clients.Group(GetGroupName(chatId)).ReceiveMessage(dto);
    }

    private static string GetGroupName(int chatId) => $"chat-{chatId}";
}