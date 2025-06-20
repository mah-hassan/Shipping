using Microsoft.EntityFrameworkCore;

namespace Shipping.Features.Orders.UpdateOrder;

public class UpdateOrderStatusEndpoint(ShippingDbContext dbContext) : Endpoint<UpdateOrderStatusRequest>
{
    public override void Configure()
    {
        Patch("/api/orders/{id}");
        Roles(nameof(AppRoles.CompanyOwner));
        Description(x => x
            .Produces<ApiResponse>()
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("orders"));
    }

    public override async Task HandleAsync(UpdateOrderStatusRequest req, CancellationToken ct)
    {
        var orderId = Route<int>("id", true);
        var userId = User.GetUserId();

        var order = await dbContext.Orders
            .Include(o => o.Company)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);

        if (order is null)
        {
            await SendAsync(ApiResponse.Failure("order", "Order not found"),
                StatusCodes.Status404NotFound, ct);
            return;
        }

        if (order.Company?.OwnerId != userId)
        {
            await SendAsync(ApiResponse.Failure("order", "You are not the owner of this order"),
                StatusCodes.Status403Forbidden, ct);
            return;
        }

        var status = Enum.Parse<OrderStatus>(req.Status, true);
        
        if (status is not OrderStatus.Shipped and not OrderStatus.Delivered)
        {
            await SendAsync(ApiResponse.Failure("order", "You can only update to Shipped or Delivered status"),
                StatusCodes.Status400BadRequest, ct);
            return;
        }
        
        if (status is OrderStatus.Shipped && order.Status != OrderStatus.Placed)
        {
            await SendAsync(ApiResponse.Failure("order", "You can only ship orders that are placed"),
                StatusCodes.Status400BadRequest, ct);
            return;
        }
        
        if (status is OrderStatus.Delivered && order.Status != OrderStatus.Shipped)
        {
            await SendAsync(ApiResponse.Failure("order", "You can only deliver orders that are shipped"),
                StatusCodes.Status400BadRequest, ct);
            return;
        }
        
        order.Status = status;
       
        var userNotification = new Notification
        {
            ReceiverId =  order.OwnerId,
            Content = $"order {order.Id} has been {status.ToString()}",
            Type = status == OrderStatus.Shipped ? NotificationType.OrderShipped : NotificationType.OrderDelivered
        };
        
        dbContext.Notifications.Add(userNotification);
        dbContext.Orders.Update(order);
        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(ApiResponse.Success(), ct);
        return;
    }
}