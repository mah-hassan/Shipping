using Microsoft.EntityFrameworkCore;

namespace Shipping.Features.Users.UpdateProfile;

public class UpdateProfileEndpoint(ShippingDbContext dbContext) : Endpoint<UpdateProfileRequest>
{
    public override void Configure()
    {
        Put("/api/users");
        Description(x => x
            .Produces<ApiResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("users"));
    }

    public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        
        user.FullName = req.FullName;
        user.Email = req.Email;
        user.PhoneNumber = req.PhoneNumber;
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(ct);
        await SendOkAsync(ApiResponse.Success(), ct);
    }
}