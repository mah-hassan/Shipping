using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace Shipping.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(value) ?
            throw new NullReferenceException("user id does not exist")
            : new Guid(value);
    }
    public static AppRoles GetRole(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.Role);
        return string.IsNullOrEmpty(value) ?
            throw new NullReferenceException("user role does not exist")
            : Enum.Parse<AppRoles>(value);
    }
}