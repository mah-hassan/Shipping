using Microsoft.EntityFrameworkCore;

namespace Shipping.Data;
public class ShippingDbContext : DbContext
{
    public ShippingDbContext(DbContextOptions<ShippingDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; } 
    public DbSet<Role> Roles { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Offer> Offers { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Message> Messages { get; set; }
    
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Complaint> Complaints { get; set; }
    public DbSet<PaymentInformation> PaymentInformation { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity(j => j.ToTable("UserRoles"));
    
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique(true);
    
        modelBuilder.Entity<User>()
            .HasIndex(u => u.PhoneNumber);
    
        modelBuilder.Entity<Entity>()
            .UseTpcMappingStrategy();
    
        // Configure Offer relationship with Order
        modelBuilder.Entity<Offer>()
            .HasOne(o => o.Order)
            .WithMany(order => order.Offers)
            .HasForeignKey(o => o.OrderId)
            .OnDelete(DeleteBehavior.ClientCascade);
    
        modelBuilder.Entity<Offer>()
            .Property(o => o.Price)
            .HasPrecision(18, 2);

        // Configure Chat relationships with Users
        modelBuilder.Entity<Chat>()
            .HasOne(c => c.ParticipantOne)
            .WithMany()
            .HasForeignKey(c => c.ParticipantOneId)
            .OnDelete(DeleteBehavior.ClientCascade);  

        modelBuilder.Entity<Chat>()
            .HasOne(c => c.ParticipantTwo)
            .WithMany()
            .HasForeignKey(c => c.ParticipantTwoId)
            .OnDelete(DeleteBehavior.ClientCascade);  
        
        // payment information
        modelBuilder.Entity<PaymentInformation>()
            .Property(o => o.Amount)
            .HasPrecision(18, 2);
        
        // reviews
        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);
    }

}