using Microsoft.EntityFrameworkCore;

namespace Shipping.Features.Offers.GetOffers;

public class GetOffersEndpoint(ShippingDbContext dbContext, IMapper mapper) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/offers");
        Roles(nameof(AppRoles.Customer), nameof(AppRoles.CompanyOwner));
        Description(x => x
            .Produces<ApiResponse<List<OfferResponse>>>()
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("offers"));
    }
    
    public override Task HandleAsync(CancellationToken ct)
    {
        var role = User.GetRole();
        if (role is AppRoles.Customer)
        {
            var orderId = Query<int>("orderId");
            return GetOffersForOrder(orderId, ct);
        }
        else if (role is AppRoles.CompanyOwner)
        {
            return GetOffersForCompany(ct);
        }
        else
        {
            return SendAsync(ApiResponse.Failure("role", "You are not authorized to get offers"),
                StatusCodes.Status403Forbidden, ct);
        }
        
    }

    private async Task GetOffersForOrder(int orderId, CancellationToken ct)
    {
        var order = await dbContext.Orders.FindAsync(orderId, ct);
        
        if (order is null)
        {
            await SendAsync(ApiResponse.Failure("order", "Order not found"),
                StatusCodes.Status404NotFound, ct);
            return;
        }
        
        var userId = User.GetUserId();
        
        if(order.OwnerId != userId)
        {
            await SendAsync(ApiResponse.Failure("order", "You are not the owner of this order"),
                StatusCodes.Status403Forbidden, ct);
            return;
        }
        
        if (order.Status != OrderStatus.Pending)
        {
            await SendAsync(ApiResponse.Failure("order", "You can only get offers for pending orders"),
                StatusCodes.Status400BadRequest, ct);
            return;
        }
        
        var offers = await dbContext.Offers
            .Where(o => o.OrderId == orderId)
            .Include(o => o.Company)
            .Include(o => order)
            .ThenInclude(order => order.Owner)
            .AsSplitQuery()
            .AsNoTracking()
            .ProjectToType<OfferResponse>()
            .ToListAsync(ct);
        
        await SendOkAsync(offers, ct);
        return;
    }
    private async Task GetOffersForCompany(CancellationToken ct)
    {
        var companyOwnerId = User.GetUserId();
        var company = await dbContext.Companies
            .FirstOrDefaultAsync(c => c.OwnerId == companyOwnerId, ct);
        
        if (company is null)
        {
            await SendAsync(ApiResponse.Failure("company", "Company not found"),
                StatusCodes.Status404NotFound, ct);
            return;
        }
        
        var offers = await dbContext.Offers
            .Where(o => o.CompanyId == company.Id)
            .Include(o => o.Order)
            .ThenInclude(order => order.Owner)
            .AsSplitQuery()
            .AsNoTracking()
            .ProjectToType<OfferResponse>()
            .ToListAsync(ct);
        
        await SendOkAsync(offers, ct);
        return;
    }
}