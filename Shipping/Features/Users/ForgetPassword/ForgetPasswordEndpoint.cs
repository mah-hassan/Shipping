using System.Text;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using OtpNet;

namespace Shipping.Features.Users.ForgetPassword;

public class ForgetPasswordEndpoint(ShippingDbContext dbContext, IFluentEmail fluentEmail) : Endpoint<ForgetPasswordRequest>
{
    private const int OtpLength = 4;
    private const int Step = 5 * 60; // 5 minutes
    public override void Configure()
    {
        Post("/api/users/forget-password");
        Description(x => x
            .Produces<ApiResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags("users"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(ForgetPasswordRequest req, CancellationToken ct)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == req.Email, ct);
        if (user is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
       
        var totp = new Totp(
            Encoding.UTF8.GetBytes(user.Id.ToString()),
            step: Step,
            totpSize: OtpLength);
        
        var otp = totp.ComputeTotp();
        var emailMessage = $"confirmation code: {otp}";
        await fluentEmail.To(user.Email)
            .Subject("Forget Password")
            .Body(emailMessage)
            .SendAsync(ct);
        await SendOkAsync(ApiResponse.Success(), ct);
    }
}
public record ForgetPasswordRequest(string Email); 