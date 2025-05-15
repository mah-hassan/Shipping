using FluentValidation;
using Shipping.Features.Users.CreateUser;

public class CreateUserValidator : Validator<CreateUserRequest>
{

    public CreateUserValidator()
    {
        RuleFor(u => u.Email)
            .EmailAddress()
            .NotEmpty();

        RuleFor(u => u.Password)
            .NotEmpty();

        RuleFor(u => u.FullName)
            .NotEmpty();
    }
}