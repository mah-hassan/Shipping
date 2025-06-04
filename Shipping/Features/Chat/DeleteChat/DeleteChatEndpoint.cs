namespace Shipping.Features.Chat.DeleteChat;

public class DeleteChatEndpoint(ShippingDbContext dbContext) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/chats/{chatId}");
        Description(x => x
            .WithTags("Chat")
            .WithSummary("Delete a chat")
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        int chatId = Route<int>("chatId");

        Entities.Chat? chat = await dbContext.Chats.FindAsync(chatId, ct);
        if (chat is null)
        {
            await SendAsync(ApiResponse.Failure("chat", "Chat not found"), 
                StatusCodes.Status404NotFound, cancellation: ct);
            return;
        }
        
        var userId = User.GetUserId();
        if(chat.ParticipantOneId != userId && chat.ParticipantTwoId != userId)
        {
            await SendAsync(ApiResponse.Failure("chat", "You are not a participant in this chat"),
                StatusCodes.Status403Forbidden, cancellation: ct);
            return;
        }
        dbContext.Chats.Remove(chat);
        await dbContext.SaveChangesAsync(ct);
        await SendOkAsync(ApiResponse.Success(), ct);
    }
}