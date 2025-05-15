namespace Shipping.Features.Companies.CreateCompany;

public class CreateCompanyRequest
{
    public required string Name { get; set; } 
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string MainAddress { get; set; }
    public required string ZipCode { get; set; }
    public required string TaxNumber { get; set; }
    public required string ResponsibleManger { get; set; } 
    public TimeSpan WorkTime { get; set; }
    public IFormFile Logo { get; set; } 
    public IFormFile TradeLicense { get; set; } 
    
    // additional information (optional)
    public IFormFileCollection? Photos { get; set; } 
    public string? About { get; set; }
    public string? Description { get; set; }
    public string? Advantages { get; set; }
    public string? Disadvantages { get; set; }
    // owner authentication information
    public required string Password { get; set; } 
}