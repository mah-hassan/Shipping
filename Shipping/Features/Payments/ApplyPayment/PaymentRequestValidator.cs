using FluentValidation;

namespace Shipping.Features.Payments.ApplyPayment;

public class PaymentRequestValidator : Validator<PaymentRequest>
{
    public PaymentRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required.");

        RuleFor(x => x.CardNumber)
            .NotEmpty()
            .WithMessage("Card number is required.")
            .CreditCard()
            .WithMessage("Invalid card number.");

        RuleFor(x => x.ExpirationMonth)
            .InclusiveBetween(1, 12)
            .WithMessage("Expiration month must be between 1 and 12.");

        RuleFor(x => x.ExpirationYear)
            .GreaterThan(DateTime.UtcNow.Year)
            .WithMessage("Expiration year must be in the future.");

        RuleFor(x => x.Cvv)
            .NotEmpty()
            .WithMessage("CVV is required.")
            .Length(3)
            .WithMessage("CVV must be 3 digits.");
    }
}