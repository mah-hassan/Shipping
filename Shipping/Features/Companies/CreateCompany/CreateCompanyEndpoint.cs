using Microsoft.EntityFrameworkCore;

namespace Shipping.Features.Companies.CreateCompany;

public class CreateCompanyEndpoint(ShippingDbContext dbContext,
    PasswordHasher passwordHasher,
    IMapper mapper,
    IFileService fileService) : Endpoint<CreateCompanyRequest>
{
    private const string CompaniesDirectory = "companies";
    public override void Configure()
    {
        Post("/api/companies");
        AllowFormData();
        Roles(nameof(AppRoles.Admin));
        Description(x => x
            .Produces<ApiResponse<CompanyResponse>>()
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .WithTags("companies"));
    }

    public override async Task HandleAsync(CreateCompanyRequest req, CancellationToken ct)
    {
        Company? company = await dbContext.Companies.FirstOrDefaultAsync(c => c.Name == req.Name, ct);
       
        User? owner = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == req.Email, ct);
        if (company is not null || owner is not null)
        {
            await SendAsync(ApiResponse.Failure("Company", "Company already exists"),
                StatusCodes.Status400BadRequest, ct);
            return;
        }
        company = mapper.Map<Company>(req);
        try
        {
            var companyAssetsDirectory = $"{CompaniesDirectory}/{company.Id}";
            company.Logo = await fileService.SaveFileAsync(req.Logo, companyAssetsDirectory);
            company.TradeLicense = await fileService.SaveFileAsync(req.TradeLicense, companyAssetsDirectory);
            if (req.Photos is not null)
            {
                foreach (var photo in req.Photos)
                {
                    var filePath = await fileService.SaveFileAsync(photo, companyAssetsDirectory);
                    company.Photos.Add(filePath);
                }
            }
            company.Owner = await CreateOwnerAccount(req);
            company.Status = CompanyStatus.Active;
            dbContext.Companies.Add(company);
            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception)
        {
            fileService.DeleteFile(company.Logo);
            fileService.DeleteFile(company.TradeLicense);
            foreach (var photo in company.Photos)
            {
                fileService.DeleteFile(photo);
            }
            await SendAsync(ApiResponse.Failure("Company", "Error creating company"), 
                StatusCodes.Status500InternalServerError, ct);
            throw;
        }
        var response = mapper.Map<CompanyResponse>(company);
        response.Logo = fileService.GetPublicUrl(company.Logo);
        response.TradeLicense = fileService.GetPublicUrl(company.TradeLicense);
        response.Photos = company.Photos.Select(p => fileService.GetPublicUrl(p)).ToList();
        await SendOkAsync(ApiResponse.Success(response), ct);
    }
    private async Task<User> CreateOwnerAccount(CreateCompanyRequest req)
    {
        var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == nameof(AppRoles.CompanyOwner));
        if (role is null)
            throw new InvalidOperationException("can not create owner account, role not found");
        var owner = new User
        {
            Email = req.Email,
            PasswordHash = passwordHasher.Hash(req.Password),
            Roles = [role],
            PhoneNumber = req.PhoneNumber,
            FullName = req.Name
        };
        dbContext.Users.Add(owner);
        return owner;
    }
}