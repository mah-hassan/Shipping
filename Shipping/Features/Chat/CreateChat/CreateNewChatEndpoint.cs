using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace Shipping.Features.Chat.CreateChat;

public class CreateNewChatEndpoint(ShippingDbContext dbContext) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/api/chats");

        Description(x => x
            .WithName("CreateNewChat")
            .WithDescription("Create a new chat or retrieve an existing one, if no parameters are provided, it will create a chat with the admin")
            .Produces<ApiResponse<ChatResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .WithTags("Chat"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        int? companyId = Query<int?>("companyId", false);
        int? orderId = Query<int?>("orderId", false);

        if (companyId is not null)
        {
            await ChatWithCompany(companyId.Value, ct);
            return;
        }
        else if (orderId is not null)
        {
            await ChatWithOrderOwner(orderId.Value, ct);
            return;
        }
        else
        {
            await ChatWithAdmin(ct);
        }
    }

    private async Task ChatWithAdmin(CancellationToken ct)
    {
        var admin = await dbContext.Users
            .Where(u => u.Roles.Any(r => r.Name == nameof(AppRoles.Admin)))
            .FirstOrDefaultAsync(ct);

        if (admin is null)
        {
            await SendAsync(ApiResponse.Failure("admin", "Admin not found"),
                StatusCodes.Status404NotFound, cancellation: ct);
            return;
        }

        var userId = User.GetUserId();
        if (await PreventSelfChat(userId, admin.Id, ct)) return;

        await CreateOrRetrieveChatAsync(userId, admin.Id, ct);
    }

    private async Task ChatWithOrderOwner(int orderId, CancellationToken ct)
    {
        var orderOwner = await dbContext.Orders
            .Include(o => o.Owner)
            .Where(o => o.Id == orderId)
            .Select(o => o.Owner)
            .FirstOrDefaultAsync(ct);

        if (orderOwner is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var userId = User.GetUserId();
        if (await PreventSelfChat(userId, orderOwner.Id, ct)) return;

        await CreateOrRetrieveChatAsync(userId, orderOwner.Id, ct);
    }

    private async Task ChatWithCompany(int companyId, CancellationToken ct)
    {
        var userId = User.GetUserId();

        var comapny = await dbContext.Companies
            .Where(c => c.Id == companyId)
            .FirstOrDefaultAsync(ct);

        if (comapny is null)
        {
            await SendAsync(ApiResponse.Failure("company", "Company not found"),
                StatusCodes.Status404NotFound, cancellation: ct);
            return;
        }

        if (await PreventSelfChat(userId, comapny.OwnerId, ct)) return;

        await CreateOrRetrieveChatAsync(userId, comapny.OwnerId, ct);
    }

    private async Task CreateOrRetrieveChatAsync(int userId, int otherUserId, CancellationToken ct)
    {
        var chat = await dbContext.Chats
            .Include(c => c.ParticipantOne)
            .Include(c => c.ParticipantTwo)
            .Include(c => c.Messages.OrderBy(m => m.CreatedAtUtc))
            .ThenInclude(m => m.Sender)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                (c.ParticipantOneId == userId && c.ParticipantTwoId == otherUserId) ||
                (c.ParticipantTwoId == userId && c.ParticipantOneId == otherUserId), ct);

        if (chat is null)
        {
            chat = new Entities.Chat
            {
                ParticipantOneId = userId,
                ParticipantTwoId = otherUserId
            };
            dbContext.Chats.Add(chat);
            await dbContext.SaveChangesAsync(ct);
        }

        var resident = chat.ParticipantOneId == userId
            ? chat.ParticipantTwo
            : chat.ParticipantOne;

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
        
         await SendAsync(ApiResponse.Success(response), cancellation: ct);
    }

    private async Task<bool> PreventSelfChat(int userId, int targetId, CancellationToken ct)
    {
        if (userId == targetId)
        {
            await SendAsync(ApiResponse.Failure("user", "You cannot chat with yourself"),
                StatusCodes.Status400BadRequest, cancellation: ct);
            return true;
        }
        return false;
    }
}