using Microsoft.EntityFrameworkCore;
using Shipping.Features.Orders.CreateOrder;

namespace Shipping.Features.Orders.UpdateOrder;

public class UpdateOrderEndpoint(ShippingDbContext dbContext) : Endpoint<OrderRequest>
{
    public override void Configure()
    {
        Put("/api/orders/{id}");
        Roles(nameof(AppRoles.Customer));
        Description(x => x
            .Produces<ApiResponse>()
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("orders"));
    }

    public override async Task HandleAsync(OrderRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var orderId = Route<Guid>("id", true);
        
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

        if (order.Status != OrderStatus.Pending)
        {
            await SendAsync(ApiResponse.Failure("order", "You can only update pending orders"),
                StatusCodes.Status400BadRequest, ct);
            return;
        }

        order.WeightInKg = req.WeightInKg;
        order.Destination = req.Destination;
        order.Details = req.Details;
        order.PickupLocation = req.PickupLocation;
        order.PackageSize = Enum.Parse<PackageSize>(req.PackageSize, true);
        
        dbContext.Orders.Update(order);
        await dbContext.SaveChangesAsync(ct);
        
        await SendOkAsync(ApiResponse.Success(), ct);
    }
}