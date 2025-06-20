using Microsoft.EntityFrameworkCore;
using Shipping.Features.Offers.GetOffers;

namespace Shipping.Features.Offers.CreateOffer;

public class CreateOfferEndpoint(ShippingDbContext dbContext, IMapper mapper) : Endpoint<OfferRequest>
{
    public override void Configure()
    {
        Post("/api/offers");
        Roles(nameof(AppRoles.CompanyOwner));
        Description(x => x
            .Produces<ApiResponse<OfferResponse>>()
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("offers"));
    }

    public override async Task HandleAsync(OfferRequest req, CancellationToken ct)
    {
        var order = await dbContext.Orders.Include(o => o.Owner)
            .FirstOrDefaultAsync(o => o.Id == req.OrderId, ct);
        
        if (order is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        
        if (order.Status != OrderStatus.Pending)
        {
            await SendAsync(ApiResponse.Failure("Order", "You can only create an offer for a pending order"),
                StatusCodes.Status400BadRequest, ct);
            return;
        }

        var companyOwnerId = User.GetUserId();
        var company = await dbContext.Companies
            .FirstOrDefaultAsync(c => c.OwnerId == companyOwnerId, ct);
    
        if (company is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        
        var offer = mapper.Map<Offer>(req);
        offer.CompanyId = company.Id;
        offer.Company = company;
        offer.Order = order;
        offer.Status = OfferStatus.Pending;
        

        dbContext.Offers.Add(offer);
        await dbContext.SaveChangesAsync(ct);
        
        var response = mapper.Map<OfferResponse>(offer);
        await SendOkAsync(ApiResponse.Success(response), ct);
    }
}

public record OfferRequest(
    int OrderId,
    decimal Price,
    int EstimatedDeliveryTimeInDays,
    string? Notes);