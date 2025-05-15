using Microsoft.EntityFrameworkCore;
using YamlDotNet.Core.Tokens;

namespace Shipping.Features.Chat.GetAll;

public class GetAllChatsEndpoint(ShippingDbContext dbContext) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/chats");
        Description(x => x
            .WithName("GetAllChats")
            .WithTags("Chat")
            .Produces<List<ChatResponse>>(StatusCodes.Status200OK));
    }
    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        
        var chats = await dbContext.Chats
            .Where(c => c.ParticipantOneId == userId || c.ParticipantTwoId == userId)
            .Select(c => new ChatResponse(c.Id,
                c.ParticipantOneId == userId ? c.ParticipantTwo.FullName : c.ParticipantOne.FullName,
                c.LastMessageAtUtc, null))
            .ToListAsync(ct);

        await SendAsync(chats, cancellation: ct);
    }
    
}