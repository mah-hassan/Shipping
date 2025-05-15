using Microsoft.EntityFrameworkCore;

namespace Shipping.Features.Users.ChangePassword;

public class ChangePasswordEndpoint(PasswordHasher passwordHasher, ShippingDbContext dbContext) : Endpoint<ChangePasswordRequest>
{
    public override void Configure()
    {
        Patch("/api/users/change-password");
        Description(x => x
            .Produces<ApiResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags("users"));
    }
    public override async Task HandleAsync(ChangePasswordRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        if (!passwordHasher.Verify(user.PasswordHash, req.CurrentPassword))
        {
            await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
            return;
        }

        user.PasswordHash = passwordHasher.Hash(req.NewPassword);
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(ct);
        await SendOkAsync(ApiResponse.Success(), ct);
    }
}