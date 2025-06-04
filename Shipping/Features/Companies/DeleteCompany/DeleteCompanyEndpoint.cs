using Microsoft.EntityFrameworkCore;

namespace Shipping.Features.Companies.DeleteCompany;

public class DeleteCompanyEndpoint(ShippingDbContext dbContext,
    IFileService fileService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/companies/{id}");
        Roles(nameof(AppRoles.Admin), nameof(AppRoles.CompanyOwner));
        Description(x => x
            .Produces<ApiResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .WithTags("companies"));
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id", true);
        var company = await dbContext.Companies
            .Include(c => c.Owner)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
        
        if (company is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        
        if (company.OwnerId != User.GetUserId() && User.GetRole() != AppRoles.Admin)
        {
            await SendAsync(ApiResponse.Failure("role", "You are not allowed to perform this action"),
                StatusCodes.Status403Forbidden, ct);
            return;
        }
        
        dbContext.Companies.Remove(company);
        dbContext.Users.Remove(company.Owner);
        await dbContext.SaveChangesAsync(ct);
        
        fileService.DeleteFile(company.Logo);
        fileService.DeleteFile(company.TradeLicense);
        foreach (var photo in company.Photos)
        {
            fileService.DeleteFile(photo);
        }
        
        await SendOkAsync(ApiResponse.Success(), ct);
    }
    
}