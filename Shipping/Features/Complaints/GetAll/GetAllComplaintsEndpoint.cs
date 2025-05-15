using Microsoft.EntityFrameworkCore;

namespace Shipping.Features.Complaints.GetAll;

public class GetAllComplaintsEndpoint(ShippingDbContext dbContext, IMapper mapper) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/complaints");
        Roles(nameof(AppRoles.Admin));
        Description(x => x
            .Produces<ApiResponse<List<ComplaintResponse>>>()
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("complaints"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var complaints = await dbContext.Complaints.ToListAsync(ct);
        var response = mapper.Map<List<ComplaintResponse>>(complaints);
        await SendOkAsync(ApiResponse.Success(response), ct);
    }
}

public class ComplaintResponse
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
}