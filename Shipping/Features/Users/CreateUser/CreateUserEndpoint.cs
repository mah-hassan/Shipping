using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Shipping.Features.Users.Shared;
using Shipping.Services;

namespace Shipping.Features.Users.CreateUser;

public class CreateUserEndpoint(ShippingDbContext dbContext,
    IJwtTokenService jwtTokenService,
    PasswordHasher passwordHasher) : Endpoint<CreateUserRequest>
{
    public override void Configure()
    {
        Post("/api/users");
        Description(x => x
            .Produces<ApiResponse<AuthResponse>>()
            .Produces<ApiResponse>(StatusCodes.Status409Conflict)
            .WithTags("users"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        User? user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == req.Email, ct);
        if (user is not null)
        {
            await SendAsync(ApiResponse.Failure("Email", $"User with '{req.Email}' Email already exists"), 
                StatusCodes.Status409Conflict, ct);
            return;
        }
        
        user = new User()
        {
            Email = req.Email,
            PhoneNumber = req.PhoneNumber,
            FullName = req.FullName,
            PasswordHash = passwordHasher.Hash(req.Password)
        };
        
        var userRole = await dbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == nameof(AppRoles.Customer), ct);
     
        if (userRole is null)
            throw new InvalidOperationException("can not create user, role not found");
        
        user.Roles.Add(userRole);

        dbContext.Users.Add(user);
        
        await dbContext.SaveChangesAsync(ct);

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