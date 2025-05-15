using Microsoft.EntityFrameworkCore;
using Shipping.Features.Companies.CreateCompany;

namespace Shipping.Features.Companies.GetAll;

public class GetAllCompaniesEndpoint(ShippingDbContext dbContext,
    IFileService fileService,
    IMapper mapper) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/companies");
        Roles(nameof(AppRoles.Admin), nameof(AppRoles.Customer));
        Description(x => x
            .Produces<ApiResponse<List<CompanyResponse>>>()
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .WithTags("companies"));
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var role = User.GetRole();
        var companiesQuery = dbContext.Companies
            .Include(c => c.Owner).AsNoTracking();

        List<Company> companies;
        if (role == AppRoles.Customer)
        {
            companies = await companiesQuery.Where(c => c.Status == CompanyStatus.Active).ToListAsync(ct);
        }
        else if (role == AppRoles.Admin)
        {
            companies = await companiesQuery.ToListAsync(ct);
        }
        else
        {
            await SendAsync(ApiResponse.Failure("role", "You are not allowed to access this endpoint"),
                StatusCodes.Status403Forbidden, ct);
            return;
        }

        var response = new List<CompanyResponse>();
        foreach (var company in companies)
        {
            var companyResponse = mapper.Map<CompanyResponse>(company);
            response.Add(companyResponse);
            companyResponse.Logo = fileService.GetPublicUrl(company.Logo);
            companyResponse.TradeLicense = fileService.GetPublicUrl(company.TradeLicense);
            companyResponse.Photos = company.Photos.Select(p => fileService.GetPublicUrl(p)).ToList();
        }
        
        await SendOkAsync(ApiResponse.Success(response), ct);
    }
}