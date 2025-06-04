using Microsoft.EntityFrameworkCore;

namespace Shipping.Features.Complaints.UpdateStatus;

public class UpdateComplaintStatusEndpoint(ShippingDbContext dbContext) : Endpoint<UpdateComplaintStatusRequest>
{
    public override void Configure()
    {
        Patch("/api/complaints/{id}");
        Roles(nameof(AppRoles.Admin));
        Description(x => x
            .Produces<ApiResponse>()
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("complaints"));
    }
    public override async Task HandleAsync(UpdateComplaintStatusRequest req, CancellationToken ct)
    {
        var complaintId = Route<int>("id", true);
        var complaint = await dbContext.Complaints
            .Include(c => c.AgainstCompany)
            .FirstOrDefaultAsync(c => c.Id == complaintId, ct);

        if (complaint is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        complaint.Status = Enum.Parse<ComplaintStatus>(req.Status, true);;
        if (complaint.Status is ComplaintStatus.Resolved)
        {
            var notification = new Notification
            {
                ReceiverId = complaint.SenderId,
                Content = $"Your complaint against {complaint.AgainstCompany.Name} has been resolved",
                Type = NotificationType.ComplaintResolved
            };
            dbContext.Notifications.Add(notification);
        }
        
        await dbContext.SaveChangesAsync(ct);
        
        await SendOkAsync(ApiResponse.Success(), ct);
    }

}
public record UpdateComplaintStatusRequest(string Status);