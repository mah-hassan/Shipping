using Microsoft.EntityFrameworkCore;

namespace Shipping.Features.Complaints.CreateComplaint;

public class CreateComplaintEndpoint(ShippingDbContext dbContext, IMapper mapper) : Endpoint<ComplaintRequest>
{
    public override void Configure()
    {
        Post("/api/complaints");
        Roles(nameof(AppRoles.Customer));
        Description(x => x
            .Produces<ApiResponse>()
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .WithTags("complaints"));
    }

    public override async Task HandleAsync(ComplaintRequest req, CancellationToken ct)
    {
        var order = await dbContext.Orders
            .Include(o => o.Company)
            .Include(o => o.Owner)
            .FirstOrDefaultAsync(o => o.Id == req.OrderId, ct);
        if (order is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        if (order.Status == OrderStatus.Pending)
        {
            await SendAsync(ApiResponse.Failure("order", "You can not create a complaint for a non-Pending order"),
                StatusCodes.Status400BadRequest, ct);
            return;
        }
        
        var userId = User.GetUserId();
        if (order.OwnerId != userId)
        {
            await SendAsync(ApiResponse.Failure("order", "You are not the owner of this order"),
                StatusCodes.Status403Forbidden, ct);
            return;
        }

        var complaint = mapper.Map<Complaint>(req);

        complaint.AgainstCompanyId = order.Company!.Id;
        complaint.SenderId = order.OwnerId;
        complaint.SenderName = order.Owner.FullName;
        
        dbContext.Complaints.Add(complaint);
        
        // send notification to the company and admin
        var admin = await dbContext.Users.FirstOrDefaultAsync(u => u.Roles.Any(r => r.Name == nameof(AppRoles.Admin)), ct);
        if (admin is not null)
        {
            var adminNotification = new Notification
            {
                ReceiverId = admin.Id,
                Content = $"New complaint received from {order.Owner.FullName} against {order.Company!.Name}",
                Type = NotificationType.ComplaintReceived
            };
            dbContext.Notifications.Add(adminNotification);
        }
        
        var companyNotification = new Notification
        {
            ReceiverId = order.Company!.Id,
            Content = $"New complaint received from {order.Owner.FullName} for order {order.Id}",
            Type = NotificationType.ComplaintReceived
        };
        
        dbContext.Notifications.Add(companyNotification);
        
        await dbContext.SaveChangesAsync(ct);
        await SendOkAsync(ApiResponse.Success(), ct);
    }
}
public record ComplaintRequest(
    int OrderId,
    string Content,
    string Type
);