using Microsoft.EntityFrameworkCore;

namespace Shipping.Features.Payments.ApplyPayment;

public class ApplyPaymentEndpoint(ShippingDbContext dbContext) : Endpoint<PaymentRequest>
{
    public override void Configure()
    {
        Post("/api/Payments");
        
        Roles(nameof(AppRoles.Customer));
        
        Description(x => x
            .WithName("ApplyPayment")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags("Payments"));
    }

    public override async Task HandleAsync(PaymentRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var order = await dbContext.Orders
            .Include(o => o.Offers.FirstOrDefault(of => of.Status == OfferStatus.Accepted))
            .FirstOrDefaultAsync(o => o.Id == req.OrderId, ct);
        
        if (order is null)
        {
            await SendAsync(ApiResponse.Failure("order", "Order not found"), 
                StatusCodes.Status404NotFound, ct);
            return;
        }

        if (order.OwnerId != userId)
        {
            await SendAsync(ApiResponse.Failure("order", "You are not the owner of this order"),
                StatusCodes.Status403Forbidden, ct);
            return;
        }
        
        var paymentInfo = await dbContext.PaymentInformation
            .FirstOrDefaultAsync(p => p.OrderId == req.OrderId, ct);

        if (paymentInfo is null)
        {
            await SendAsync(ApiResponse.Failure("order", "you have to accept the offer first"),
                StatusCodes.Status400BadRequest, ct);
            return;
        }

        paymentInfo.Status = PaymentStatus.Completed;
        paymentInfo.LastFourDigits = req.CardNumber.Substring(req.CardNumber.Length - 4);
        paymentInfo.UpdatedAtUtc = DateTime.UtcNow;

        order.Status = OrderStatus.Placed;
        var offer = order.Offers[0];
        offer.DeliveryDateUtc = DateTime.UtcNow.AddDays(offer.EstimatedDeliveryTimeInDays);
                
        var userNotification = new Notification
        {
            ReceiverId =  userId,
            Content = $"order {order.Id} has been placed",
            Type = NotificationType.OrderPlaced
        };
        
        dbContext.Notifications.Add(userNotification);
        
        dbContext.PaymentInformation.Update(paymentInfo);
        dbContext.Orders.Update(order);
        dbContext.Offers.Update(offer);
        await dbContext.SaveChangesAsync(ct);
        
        await SendOkAsync(ApiResponse.Success(), ct);
    }
}