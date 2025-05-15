
using Microsoft.EntityFrameworkCore;


namespace Shipping;
public class DatabaseInitializer(IServiceScope scope)
{
    public async Task InitializeAsync()
    {
        using var context = scope.ServiceProvider.GetRequiredService<ShippingDbContext>();
       
        // await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
        await SeedRolesAsync(context);
        await SeedUsersAsync(context);
    }

    private async Task SeedUsersAsync(ShippingDbContext context)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }
        var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();
        
        var adminRole = await context.Roles.FirstAsync(r => r.Name == nameof(AppRoles.Admin));
        var users = new List<User>
        {
            new User
            {
                FullName = "Admin",
                PhoneNumber = "01234567890",
                Email = "admin@test.com",
                PasswordHash = passwordHasher.Hash("admin123"),
                Roles = [adminRole]
            }
        };
        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }

    private async Task SeedRolesAsync(ShippingDbContext context)
    {
        if(await context.Roles.AnyAsync())
        {
            return;
        }
        
        var roles = Enum.GetValues<AppRoles>().Select(role => new Role()
        {
            Name = role.ToString()
        });
        
        context.Roles.AddRange(roles);
        await context.SaveChangesAsync();
    }
}
