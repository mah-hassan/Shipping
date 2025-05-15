namespace Shipping.Features.Payments.ApplyPayment;

public record PaymentRequest(Guid OrderId,
    string CardNumber,
    int ExpirationMonth,
    int ExpirationYear,
    string Cvv);