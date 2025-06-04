using Microsoft.EntityFrameworkCore;
using Shipping.Features.Companies.CreateCompany;

namespace Shipping.Features.Companies.GetById;

public class GetCompanyByIdEndpoint(ShippingDbContext dbContext,
    IFileService fileService,
    IMapper mapper) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/companies/{id}");
        Roles(nameof(AppRoles.Admin), nameof(AppRoles.CompanyOwner));
        Description(x => x
            .Produces<ApiResponse<CompanyResponse>>()
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("companies"));
    }
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id", true);
        var company = await dbContext.Companies
            .Include(c => c.Owner)
            .Include(c => c.Reviews)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
        
        if (company is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var response = mapper.Map<CompanyResponse>(company);
        response.Logo = fileService.GetPublicUrl(company.Logo);
        response.TradeLicense = fileService.GetPublicUrl(company.TradeLicense);
        response.Photos = company.Photos.Select(p => fileService.GetPublicUrl(p)).ToList();
        await SendOkAsync(ApiResponse.Success(response), ct);
    }
    
}