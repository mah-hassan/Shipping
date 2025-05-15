using Microsoft.EntityFrameworkCore;

namespace Shipping.Features.Notifications.GetNotifications;

public class GetNotificationsEndpoint(ShippingDbContext dbContext, IMapper mapper) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/notifications");
        Description(x => x
            .Produces<ApiResponse<List<NotificationResponse>>>()
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("notifications"));
    }
    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var notifications = await dbContext.Notifications
            .Where(n => n.ReceiverId == userId)
            .ToListAsync(ct);
        var response = mapper.Map<List<NotificationResponse>>(notifications);
        await SendOkAsync(ApiResponse.Success(response), ct);
    }
}
public record NotificationResponse(string Content, DateTime CreatedAtUtc);