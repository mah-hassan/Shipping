using Shipping.Features.Companies;
using Shipping.Features.Companies.CreateCompany;
using Shipping.Features.Complaints.GetAll;
using Shipping.Features.Offers.GetOffers;
using Shipping.Features.Orders.GetOrders;
using Shipping.Features.Reviews.GetReviews;


namespace Shipping.Shared;

public class MappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<Order, OrderResponse>()
            .Map(dest => dest.OwnerName,
                src => src.Owner.FullName)
            .Map(dest => dest.CompanyName,
                src => src.Company != null ? src.Company.Name : null);
        
        config
            .NewConfig<Company, CompanyResponse>()
            .Map(dest => dest.OwnerName,
                src => src.Owner.FullName)
            .Map(dest => dest.OwnerEmail,
                src => src.Owner.Email)
            .Map(dest => dest.OwnerPhoneNumber,
                src => src.Owner.PhoneNumber)
            .Map(dest => dest.AverageRating,
                src => src.Reviews.Any() ? (double)Math.Min(src.Reviews.Average(r => r.Rating), 5) : 0)
            .Ignore(dest => dest.Logo)
            .Ignore(dest => dest.TradeLicense)
            .Ignore(dest => dest.Photos);
     
        config
            .NewConfig<CreateCompanyRequest, Company>()
            .Ignore(dest => dest.Logo)
            .Ignore(dest => dest.TradeLicense)
            .Ignore(dest => dest.Photos);

        config
            .NewConfig<Offer, OfferResponse>()
            .Map(dest => dest.CompanyName,
                src => src.Company.Name)
            .Map(dest => dest.CustomerName,
                src => src.Order.Owner.FullName);

        config.NewConfig<Complaint, ComplaintResponse>()
            .Map(dest => dest.CompanyName, src => src.AgainstCompany.Name)
            .Map(dest => dest.CompanyId, src => src.AgainstCompanyId);

        config.NewConfig<Review, ReviewResponse>()
            .Map(dest => dest.UserName, src => src.User.FullName);
    }
}