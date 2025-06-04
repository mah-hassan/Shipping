namespace Shipping.Features.Payments.ApplyPayment;

public record PaymentRequest(int OrderId,
    string CardNumber,
    int ExpirationMonth,
    int ExpirationYear,
    string Cvv);