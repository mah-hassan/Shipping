using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Shipping.Features.Offers.UpdateOffer;

public class UpdateOfferStatusEndpoint(ShippingDbContext dbContext) : Endpoint<UpdateOfferStatusRequest>
{
    public override void Configure()
    {
        Patch("/api/offers/{id}");
        Roles(nameof(AppRoles.Customer));
        Description(x => x
            .Produces<ApiResponse>()
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("offers"));
    }
    public override async Task HandleAsync(UpdateOfferStatusRequest req, CancellationToken ct)
    {
        var offerId = Route<int>("id", true);
        var userId = User.GetUserId();
       
        var offer = await dbContext.Offers
            .Include(o => o.Order)
            .FirstOrDefaultAsync(o => o.Id == offerId, ct);
    
        if (offer is null)
        {
            await SendAsync(ApiResponse.Failure("offer", "Offer not found"),
                StatusCodes.Status404NotFound, ct);
            return;
        }

        if (offer.Order.OwnerId != userId)
        {
            await SendAsync(ApiResponse.Failure("order", "You are not the owner of this order"),
                StatusCodes.Status403Forbidden, ct);
            return;
        }
        
  
        if (offer.Status != OfferStatus.Pending)
        {
            await SendAsync(ApiResponse.Failure("offer", "You can only update pending offers"),
                StatusCodes.Status400BadRequest, ct);
            return;
        }
        
        var status = Enum.Parse<OfferStatus>(req.Status, true);
        if (status is OfferStatus.Accepted && string.IsNullOrEmpty(req.PaymentMethod))
        {
            await SendAsync(ApiResponse.Failure("paymentMethod", "Payment method is required"),
                StatusCodes.Status400BadRequest, ct);
            return;
        }
        var paymentMethod = Enum.Parse<PaymentMethod>(req.PaymentMethod, true);
        if(status is OfferStatus.Accepted)
        {
            await AcceptOfferAsync(offer, paymentMethod, ct);
        }
        else if (status is OfferStatus.Rejected)
        {
            offer.Status = OfferStatus.Rejected;
            
            var companyNotification = new Notification
            {
                ReceiverId = offer.CompanyId,
                Content = $"offer rejected for order {offer.OrderId}",
                Type = NotificationType.OfferRejected
            };
            
            dbContext.Notifications.Add(companyNotification);
            dbContext.Offers.Update(offer);
            await dbContext.SaveChangesAsync(ct);
        }
        else
        {
            await SendAsync(ApiResponse.Failure("offer", "Invalid status"),
                StatusCodes.Status400BadRequest, ct);
            return;
        }
        
        await SendOkAsync(ApiResponse.Success(), ct);
    }

    private async Task AcceptOfferAsync(Offer offer, PaymentMethod paymentMethod, CancellationToken ct)
    {
        if (paymentMethod is PaymentMethod.CashOnDelivery)
        {
            offer.Status = OfferStatus.Accepted;
            offer.DeliveryDateUtc = DateTime.UtcNow.AddDays(offer.EstimatedDeliveryTimeInDays);
            offer.Order.Status = OrderStatus.Placed;
         
            var companyNotification = new Notification
            {
                ReceiverId = offer.CompanyId,
                Content = $"offer accepted for order {offer.OrderId}",
                Type = NotificationType.OfferAccepted
            };
            var userNotification = new Notification
            {
                ReceiverId = offer.Order.OwnerId,
                Content = $"order {offer.OrderId} has been placed",
                Type = NotificationType.OrderPlaced
            };
            dbContext.Notifications.Add(userNotification);
            dbContext.Notifications.Add(companyNotification);
        }
        else if (paymentMethod is PaymentMethod.CreditCard)
        {
            offer.Status = OfferStatus.Accepted;
            offer.Order.Status = OrderStatus.PendingPayment;
            var paymentInfo = new PaymentInformation()
            {
                Status = PaymentStatus.Pending,
                OrderId = offer.OrderId
            };
            dbContext.PaymentInformation.Add(paymentInfo);
        }
    
        dbContext.Offers.Update(offer);
        dbContext.Orders.Update(offer.Order);
        await dbContext.SaveChangesAsync(ct);
        
        await dbContext.Offers
            .Where(o => o.OrderId == offer.OrderId && o.Id != offer.Id)
            .ExecuteUpdateAsync(x 
                => x.SetProperty(o => o.Status, OfferStatus.Expired), ct);
    }
}
public record UpdateOfferStatusRequest(string Status, string? PaymentMethod);

public class UpdateOfferStatusRequestValidator : Validator<UpdateOfferStatusRequest>
{
    public UpdateOfferStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required")
            .IsEnumName(typeof(OfferStatus), caseSensitive: false)
            .WithMessage("Invalid status");
        
        RuleFor(x => x.PaymentMethod)
            .IsEnumName(typeof(PaymentMethod), caseSensitive: false)
            .WithMessage("Invalid payment method");
    }
}