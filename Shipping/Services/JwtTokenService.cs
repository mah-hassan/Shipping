using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Shipping.Services;

public class JwtTokenService(IConfiguration configuration, ShippingDbContext dbContext) : IJwtTokenService
{
    private readonly IConfiguration _configuration = configuration;

    private readonly string _secret = configuration["Jwt:Secret"]!;
    private readonly string _issuer = configuration["Jwt:Issuer"]!;
    private readonly string _audience = configuration["Jwt:Audience"]!;

    public async Task<string> GenerateToken(User user, DateTime? expires = null, params List<Claim> claims)
    { 
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // load roles navigation property in user if it is not loaded
        if (!dbContext.Entry(user).Collection(u => u.Roles).IsLoaded)
        {
            await dbContext.Entry(user).Collection(u => u.Roles).LoadAsync();
        }

        List<Claim> baseClaims = [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.FullName),
            new Claim(ClaimTypes.Role, string.Join(',', user.Roles.Select(r => r.Name))),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];
        
        if (!claims.Any())
        {
            claims = baseClaims;
        }
        else
        {
            claims.AddRange(baseClaims);
        }
     

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(5), // just in development,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}