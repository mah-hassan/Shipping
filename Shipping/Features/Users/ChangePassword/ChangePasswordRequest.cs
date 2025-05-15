namespace Shipping.Features.Users.ChangePassword;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);