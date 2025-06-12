/////////////////finally//////////////////////////////////////////

using System.Net;

using System.Net.Mail;
using System.Security.Claims;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Shipping;
using Shipping.Hubs.ChatHub;
using Shipping.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.
builder.Services.AddHttpContextAccessor();

//builder.Services.AddFastEndpoints()
//    .SwaggerDocument();
builder.Services.AddFastEndpoints()
    .SwaggerDocument(o =>
    {
        o.DocumentSettings = s =>
        {
            s.DocumentName = "v1";
            s.Title = "Shipping API";
            s.Version = "v1";
        };
    });
/////////////////finally//////////////////////////////////////////


builder.Services.AddIdentityServices(configuration);

builder.Services.AddDbContext<ShippingDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
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
//builder.Services.AddCors();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    // Enable HTTP on port 5000
//    serverOptions.ListenAnyIP(5000); // HTTP

//    // Enable HTTPS on port 5001
//    serverOptions.ListenAnyIP(5001, listenOptions =>
//    {
//        listenOptions.UseHttps(); // يستخدم الشهادة الافتراضية
//    });
//});

var app = builder.Build();

//app.UseSwagger();
//app.UseSwaggerUI(c => c.EnableTryItOutByDefault());

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

using IServiceScope scope = app.Services.CreateScope();

DatabaseInitializer databaseInitializer = new(scope);
await databaseInitializer.InitializeAsync();
//app.UseCors(policy =>
//    policy.WithOrigins("http://localhost:3000")
//          .AllowAnyMethod()
//          .AllowAnyHeader()
//          .AllowCredentials());

app.UseCors("AllowFrontend");


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
app.MapGet("/", () => Results.Redirect("/swagger"));
app.Run();