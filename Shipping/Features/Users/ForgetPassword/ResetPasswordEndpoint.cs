using Microsoft.EntityFrameworkCore;

namespace Shipping.Features.Users.ForgetPassword;

public class ResetPasswordEndpoint(ShippingDbContext dbContext, PasswordHasher passwordHasher) : Endpoint<ResetPasswordRequest>
{
    public override void Configure()
    {
        Post("/api/users/reset-password");
        Description(x => x
            .Produces<ApiResponse>()
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("users"));
        Claims("isTemp");
    }

    public override async Task HandleAsync(ResetPasswordRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        
        if (user is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        
        user.PasswordHash = passwordHasher.Hash(req.NewPassword);
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(ct);
        await SendOkAsync(ApiResponse.Success(), ct);
    }
}
public record ResetPasswordRequest(string NewPassword);