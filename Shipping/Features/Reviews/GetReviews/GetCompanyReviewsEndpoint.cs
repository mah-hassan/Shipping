using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Shipping.Features.Reviews.GetReviews;

public class GetCompanyReviewsEndpoint(ShippingDbContext dbContext) : Endpoint<GetReviewsRequest>
{
    public override void Configure()
    {
        Get("/api/reviews");
        Roles(nameof(AppRoles.CompanyOwner), nameof(AppRoles.Customer));
        Description(x => x
            .Produces<ApiResponse<List<ReviewResponse>>>()
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("reviews"));
    }

    public override async Task HandleAsync(GetReviewsRequest req, CancellationToken ct)
    {
        var companyId = req.CompanyId;
       
        var reviews = await dbContext.Reviews.Where(r => r.CompanyId == companyId)
            .Include(r => r.User)
            .ProjectToType<ReviewResponse>()
            .ToListAsync(ct);
        
        await SendOkAsync(ApiResponse.Success(reviews), ct);
    }
}
public record GetReviewsRequest([property: QueryParam(IsRequired = true)] Guid CompanyId);