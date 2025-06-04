namespace Shipping.Features.Companies.CreateCompany;

public class CompanyResponse
{
    public required int Id { get; set; }
    public required string Name { get; set; } 
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string MainAddress { get; set; }
    public required string ZipCode { get; set; }
    public required string TaxNumber { get; set; }
    public required string ResponsibleManger { get; set; } 
    public TimeSpan WorkTime { get; set; }
    public string Logo { get; set; } 
    public string TradeLicense { get; set; } 
    
    // additional information (optional)
    public List<string> Photos { get; set; } = new();
    public string? About { get; set; }
    public string? Description { get; set; }
    public string? Advantages { get; set; }
    public string? Disadvantages { get; set; }
    // owner information
    public string OwnerName { get; set; }
    public string OwnerEmail { get; set; }
    public string OwnerPhoneNumber { get; set; }
    // reviews
    public double AverageRating { get; set; }
}