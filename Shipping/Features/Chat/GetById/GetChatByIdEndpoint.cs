using Microsoft.EntityFrameworkCore;

namespace Shipping.Features.Chat.GetById;

public class GetChatByIdEndpoint(ShippingDbContext dbContext) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/chats/{id}");

        Description(x => x
            .WithName("GetChatById")
            .WithTags("Chat")
            .Produces<ChatResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var chatId = Route<int>("id", true);
        var userId = User.GetUserId();

        var chat = await dbContext.Chats
            .Include(c => c.ParticipantOne)
            .Include(c => c.ParticipantTwo)
            .Include(c => c.Messages.OrderBy(m => m.CreatedAtUtc))
            .ThenInclude(m => m.Sender)
            .AsSplitQuery()
            .AsNoTracking()
            .Where(c => c.Id == chatId && (c.ParticipantOneId == userId || c.ParticipantTwoId == userId))
            .FirstOrDefaultAsync(ct);

        if (chat is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        var resident = chat.ParticipantOneId == userId
            ? chat.ParticipantOne
            : chat.ParticipantTwo;
       
        var response = new ChatResponse(chat.Id, resident.FullName,
            chat.LastMessageAtUtc, chat.Messages.Select(m =>
                new ChatMessageResponse()
                {
                    ChatId = chat.Id,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.FullName,
                    Content = m.Content,
                    SentAtUtc = m.CreatedAtUtc
                }).ToList());
        await SendAsync(response, cancellation: ct);
    }
}