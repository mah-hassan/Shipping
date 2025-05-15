using System.Security.Claims;

namespace Shipping.Contracts;

public interface IJwtTokenService
{
    Task<string> GenerateToken(User user, DateTime? expires = null, params List<Claim> claims);
}