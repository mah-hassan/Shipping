using Microsoft.EntityFrameworkCore;
using Shipping.Features.Offers.GetOffers;

namespace Shipping.Features.Offers.GetById;

public class GetOfferByIdEndpoint(ShippingDbContext dbContext, IMapper mapper) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/offers/{id}");
        Roles(nameof(AppRoles.Customer), nameof(AppRoles.CompanyOwner));
        Description(x => x
            .Produces<ApiResponse<OfferResponse>>()
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("offers"));
    }
    public override async Task HandleAsync(CancellationToken ct)
    {
        var offerId = Route<int>("id", true);
     
        var offer = await dbContext.Offers
            .Include(o => o.Company)
            .Include(o => o.Order)
            .ThenInclude(or => or.Owner)
            .ProjectToType<OfferResponse>()
            .FirstOrDefaultAsync(o => o.Id == offerId, ct);

        if (offer is null)
        {
            await SendAsync(ApiResponse.Failure("offer", "Offer not found"),
                StatusCodes.Status404NotFound, ct);
            return;
        }
        
        await SendOkAsync(offer, ct);
    }
}