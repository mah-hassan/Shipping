using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Shipping.Features.Users.Shared;
using Shipping.Services;

namespace Shipping.Features.Users.Login;

public class LoginEndpoint(ShippingDbContext dbContext,
    IJwtTokenService jwtTokenService,
    PasswordHasher passwordHasher) : Endpoint<LoginRequest>
{
    public override void Configure()
    {
        Post("/api/users/login");
        Description(x => x
            .Produces<ApiResponse<AuthResponse>>()
            .WithName("Login")
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("users"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await dbContext.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == req.Email, cancellationToken: ct);
        
        if (user is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var isVerified = passwordHasher.Verify(user.PasswordHash, req.Password);
        if (!isVerified)
        {
            await SendAsync(ApiResponse.Failure("Invalid Credentials", "Invalid email or password"),
                StatusCodes.Status400BadRequest, ct);
            return;
        }
        
        var token = await jwtTokenService.GenerateToken(user);
        
        var response = new AuthResponse()
        {
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Token = token
        };
        
        await SendAsync(ApiResponse.Success(response), 200, ct);
    }
}