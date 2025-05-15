namespace Shipping.Features.Users.CreateUser;

public record CreateUserRequest(string Email, string Password, string FullName, string PhoneNumber);