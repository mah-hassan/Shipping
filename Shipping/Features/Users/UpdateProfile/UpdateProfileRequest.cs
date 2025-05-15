namespace Shipping.Features.Users.UpdateProfile;

public class UpdateProfileRequest
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
}