using Microsoft.EntityFrameworkCore;

namespace Shipping.Features.Reviews.AddReview;

public class AddReviewEndpoint(ShippingDbContext dbContext) : Endpoint<ReviewRequest>
{
    public override void Configure()
    {
        Post("/api/reviews");
        Roles(nameof(AppRoles.Customer));
        Description(x => x
            .Produces<ApiResponse>()
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .WithTags("reviews"));
    }

    public override async Task HandleAsync(ReviewRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var orderId = req.OrderId;

        var order = await dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);
        
        if (order is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        if (order.OwnerId != userId)
        {
            await SendAsync(ApiResponse.Failure("order", "You are not the owner of this order"),
                StatusCodes.Status403Forbidden, ct);
            return;
        }

        if (order.Status != OrderStatus.Delivered)
        {
            await SendAsync(ApiResponse.Failure("order", "You can only review delivered orders"),
                StatusCodes.Status400BadRequest, ct);
            return;
        }

        var review = new Review
        {
            UserId = userId,
            OrderId = orderId,
            CompanyId = order.CompanyId!.Value,
            Comment = req.Comment,
            Rating = Math.Min(req.Rating, 5),
            PackageDamaged = req.PackageDamaged,
            DeliveryLate = req.DeliveryLate
        };

        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(ApiResponse.Success(), ct);
    }
}