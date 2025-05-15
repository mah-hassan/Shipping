

namespace Shipping.Features.Orders.CreateOrder;

public class CreateOrderEndpoint(ShippingDbContext dbContext,
    IHttpContextAccessor contextAccessor) : Endpoint<OrderRequest>
{
    public override void Configure()
    {
        Post("/api/orders");
        Roles(nameof(AppRoles.Customer));
        Description(x => x
            .Produces<ApiResponse>()
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .WithTags("orders"));
    }

    public override async Task HandleAsync(OrderRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        Order order = req.Adapt<Order>();
        order.OwnerId = userId;
        dbContext.Orders.Add(order);
        
        await dbContext.SaveChangesAsync(ct);
        
        await SendCreatedAtAsync("/api/orders/{id}",
            new { id = order.Id },
            ApiResponse.Success(), true, ct);
    }
}