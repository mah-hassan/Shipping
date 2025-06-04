namespace Shipping.Entities;

public abstract class Entity
{
    public int Id { get; init; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
} 