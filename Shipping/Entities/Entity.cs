namespace Shipping.Entities;

public abstract class Entity(Guid id)
{
    public Guid Id { get; init; } = id;
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
} 