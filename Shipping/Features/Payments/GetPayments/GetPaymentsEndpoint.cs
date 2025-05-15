using Microsoft.EntityFrameworkCore;

namespace Shipping.Features.Payments.GetPayments;

public class GetPaymentsEndpoint(ShippingDbContext dbContext) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/payments");
        Roles(nameof(AppRoles.CompanyOwner));
        Description(x => x
            .Produces<ApiResponse<List<PaymentResponse>>>()
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .WithTags("Payments"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        
        var company = await dbContext.Companies.FirstOrDefaultAsync(c => c.OwnerId == userId, ct);
        
        if (company is null)
        {
            await SendAsync(ApiResponse.Failure("company", "Company not found"),
                StatusCodes.Status404NotFound, ct);
            return;
        }

        var payments = await dbContext.PaymentInformation
            .Include(p => p.Order)
            .ThenInclude(o => o.Owner)
            .Where(p => p.Order.CompanyId == company.Id)
            .ToListAsync(ct);
        
        var paymentResponses = payments.Select(p => new PaymentResponse(
            p.Id,
            p.OrderId,
            p.Status.ToString(),
            p.Order.Owner.FullName,
            p.LastFourDigits,
            p.Amount,
            p.UpdatedAtUtc))
            .ToList();

        await SendOkAsync(ApiResponse.Success(paymentResponses), ct);
        return;
    }
}

public record PaymentResponse(Guid Id,
    Guid OrderId,
    string Status,
    string CustomerName,
    string? LastFourDigits,
    decimal Amount,
    DateTime? UpdatedAtUtc);