using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using Shipping.Services;

namespace Shipping.Features.Users.ForgetPassword;

public class VerifyOtpEndpoint(ShippingDbContext dbContext, IJwtTokenService jwtTokenService) : Endpoint<VerifyOtpRequest>
{
    private const int OtpLength = 4;
    private const int Step = 5 * 60; // 5 minutes
    public override void Configure()
    {
        Post("/api/users/verify-otp");
        Description(x => x
            .Produces<ApiResponse<VerifyOtpResponse>>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .WithTags("users"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(VerifyOtpRequest req, CancellationToken ct)
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
        
        var isOtpValid = totp.VerifyTotp(req.Otp, out _);
       
        if (!isOtpValid)
        {
            await SendAsync(ApiResponse.Failure("otp", "Invalid otp"),
                StatusCodes.Status400BadRequest, ct);
            return;
        }

        var tempToken = await jwtTokenService.GenerateToken(user, 
            DateTime.UtcNow.AddMinutes(5), new Claim("isTemp", "true"));
        
        await SendOkAsync(ApiResponse.Success(new VerifyOtpResponse(tempToken)), ct);
    }
}
public record VerifyOtpRequest(string Email, string Otp);
public record VerifyOtpResponse(string TempToken);