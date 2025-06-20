using Microsoft.EntityFrameworkCore;
using Shipping.Features.Orders.GetOrders;

namespace Shipping.Features.Orders.GetById;

public class GetOrderByIdEndpoint(ShippingDbContext dbContext, IMapper mapper) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/orders/{id}");
        Roles(nameof(AppRoles.Customer), nameof(AppRoles.CompanyOwner));
        Description(x => x
            .Produces<ApiResponse<OrderResponse>>()
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("orders"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orderId = Route<int>("id", true);
       
        var order = await dbContext.Orders
            .Include(o => o.Owner)
            .Include(o => o.Company)
            .Include(o => o.Offers.Where(of => of.Status == OfferStatus.Accepted))
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);
      
        if (order is null)
        {
            await SendAsync(ApiResponse.Failure("order", "Order not found"),
                StatusCodes.Status404NotFound, ct);
            return;
        }
        
        await SendOkAsync(mapper.Map<OrderResponse>(order), ct);
    }
}