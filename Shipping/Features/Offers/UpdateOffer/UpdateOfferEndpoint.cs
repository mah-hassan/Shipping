using Microsoft.EntityFrameworkCore;
using Shipping.Features.Offers.CreateOffer;

namespace Shipping.Features.Offers.UpdateOffer;

public class UpdateOfferEndpoint(ShippingDbContext dbContext) : Endpoint<OfferRequest>
{
    public override void Configure()
    {
        Put("/api/offers/{id}");
        Roles(nameof(AppRoles.CompanyOwner));
        Description(x => x
            .Produces<ApiResponse>()
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("offers"));
    }

    public override async Task HandleAsync(OfferRequest req, CancellationToken ct)
    {
        var offerId = Route<Guid>("id", true);
        var companyOwnerId = User.GetUserId();
        var company = await dbContext
            .Companies
            .FirstOrDefaultAsync(c => c.OwnerId == companyOwnerId, ct);
        if (company is null)
        {
            await SendAsync(ApiResponse.Failure("company", "You are not the owner of any company"),
                StatusCodes.Status403Forbidden, ct);
            return;
        }
      
        var offer = await dbContext.Offers
            .FindAsync(offerId, ct);
        if (offer is null)
        {
            await SendAsync(ApiResponse.Failure("offer", "Offer not found"),
                StatusCodes.Status404NotFound, ct);
            return;
        }
        if (offer.CompanyId != company.Id)
        {
            await SendAsync(ApiResponse.Failure("offer", "You are not the owner of this offer"),
                StatusCodes.Status403Forbidden, ct);
            return;
        }
        if (offer.Status != OfferStatus.Pending)
        {
            await SendAsync(ApiResponse.Failure("offer", "You can only update pending offers"),
                StatusCodes.Status400BadRequest, ct);
            return;
        }
     
        offer.Price = req.Price;
        offer.EstimatedDeliveryTimeInDays = req.EstimatedDeliveryTimeInDays;
        offer.Notes = req.Notes;
      
        dbContext.Offers.Update(offer);
        await dbContext.SaveChangesAsync(ct);
        
        await SendOkAsync(ApiResponse.Success(), ct);
    }
}