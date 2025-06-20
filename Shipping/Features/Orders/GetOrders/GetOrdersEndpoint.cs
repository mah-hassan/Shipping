using Microsoft.EntityFrameworkCore;

namespace Shipping.Features.Orders.GetOrders;

public class GetOrdersEndpoint(ShippingDbContext dbContext,
    IMapper mapper) : Endpoint<GetOrdersRequest>
{
    public override void Configure()
    {
        Get("/api/orders");
        
        Description(x => x
            .Produces<ApiResponse<List<OrderResponse>>>()
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .WithTags("orders"));
    }

    public override async Task HandleAsync(GetOrdersRequest req, CancellationToken ct)
    {
        OrderStatus? status = Enum.TryParse<OrderStatus>(req.Status, true, out var orderStatus)
            ? orderStatus
            : null;
        
        var role = User.GetRole();

        List<Order> orders = role switch
        {
            AppRoles.Customer => await GetCustomerOrdersAsync(status, ct),
            AppRoles.CompanyOwner => await GetCompanyOrdersAsync(status, ct),
            _ => await GetOrdersForAdminAsync(status, ct)
        };

        var response = mapper.Map<List<OrderResponse>>(orders);
        await SendOkAsync(ApiResponse.Success(response), ct);
    }

    private async Task<List<Order>> GetCompanyOrdersAsync(OrderStatus? status, CancellationToken ct = default)
    {
        var companyOwnerId = User.GetUserId();
        
        var company = await dbContext
            .Companies
            .FirstOrDefaultAsync(c => c.OwnerId == companyOwnerId, ct) ?? throw new InvalidOperationException("company not found");

        var ordersQuery = dbContext
            .Orders
            .Include(o => o.Owner)
            .Include(o => o.Offers.Where(of => of.Status == OfferStatus.Accepted))
            .AsQueryable();

        if(status is not null && status is not OrderStatus.Pending)
        {
            ordersQuery = ordersQuery.Where(o => o.Status == status && o.CompanyId == company.Id);
        }
        else if (status is OrderStatus.Pending)
        {
            var companyOffersOrderIds = await dbContext
                .Offers
                .Where(o => o.CompanyId == company.Id)
                .Select(o => o.OrderId)
                .ToListAsync(ct);

            ordersQuery = ordersQuery
                        .Where(o => companyOffersOrderIds.Contains(o.Id) && o.Status == OrderStatus.Pending);
        }
        else
        {
            ordersQuery = ordersQuery.Where(o => o.Status == OrderStatus.Pending);
        }

        var orders = await ordersQuery.AsNoTracking().ToListAsync(ct);
        foreach (Order order in orders)
        {
            order.Company = company;
        }
        return orders;
    }
    private Task<List<Order>> GetCustomerOrdersAsync(OrderStatus? status, CancellationToken ct = default)
    {
        var customerId = User.GetUserId();
        var ordersQuery = dbContext
            .Orders
            .Include(o => o.Owner)
            .Include(o => o.Company)
            .Where(o => o.OwnerId == customerId);
    
        if (status is not null)
            ordersQuery = ordersQuery.Where(o => o.Status == status);
        
        return ordersQuery.AsNoTracking().ToListAsync(ct);
    }

    private Task<List<Order>> GetOrdersForAdminAsync(OrderStatus? status, CancellationToken ct = default)
    {
        var ordersQuery = dbContext
            .Orders
            .Include(o => o.Owner)
            .Include(o => o.Company)
            .AsNoTracking();
       
        if (status is not null)
            ordersQuery = ordersQuery.Where(o => o.Status == status);
        
        return ordersQuery.ToListAsync(ct);
    }
}