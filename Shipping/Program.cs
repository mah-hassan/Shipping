using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Shipping;
using Shipping.Hubs.ChatHub;
using Shipping.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.
builder.Services.AddHttpContextAccessor();

builder.Services.AddFastEndpoints()
    .SwaggerDocument();

builder.Services.AddIdentityServices(configuration);

builder.Services.AddDbContext<ShippingDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("ShippingDb")));
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddScoped<IFileService, FileService>();
// Add mappers
builder.Services.AddMapsterConfiguration();

// Add SignalR
builder.Services.AddSignalR();

// add fluent email
builder.Services
    .AddFluentEmail(configuration["Email:From"], configuration["Email:Sender"])
    .AddSmtpSender(new SmtpClient(configuration["Email:Host"])
    {
        Port = configuration.GetValue<int>("Email:Port"),
        Credentials = new NetworkCredential(
            configuration["Email:From"],
            configuration["Email:Password"]
        ),
        EnableSsl = configuration.GetValue<bool>("Email:UseSsl")
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using IServiceScope scope = app.Services.CreateScope();

DatabaseInitializer databaseInitializer = new(scope);
await databaseInitializer.InitializeAsync();

app.UseAuthentication()
    .UseAuthorization()
    .UseFastEndpoints(o => o.Security.RoleClaimType = ClaimTypes.Role)
    .UseSwaggerGen();

app.MapGet("/api/{base64FilePath}", (string base64FilePath, IFileService fileService) =>
{
    try
    {
        var physicalPath = fileService.GetPhysicalPath(base64FilePath);

        if (!File.Exists(physicalPath))
        {
            return Results.NotFound();
        }

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(physicalPath, out var contentType))
        {
            // Default content type if the extension is unknown
            contentType = "application/octet-stream";
        }

        return Results.File(physicalPath, contentType);
    }
    catch (Exception ex)
    {
        return Results.Problem($"An error occurred: {ex.Message}", statusCode: 500);
    }
});
app.MapHub<ChatHub>("/hubs/chat");
app.Run();