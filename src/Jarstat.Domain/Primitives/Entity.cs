using Jarstat.Domain.Entities;

namespace Jarstat.Domain.Primitives;

public abstract class Entity : IEquatable<Entity>
{
    // TODO: figure out how to use protected constructor of the base class instead of default constructors with EF Core
    //protected Entity(Guid id) => Id = id;
    //protected Entity() { }

    public Guid Id { get; protected init; }
    public DateTime DateTimeCreated { get; protected init; }
    public DateTime DateTimeUpdated { get; protected set; }
    public User Creator { get; protected init; } = null!;
    public User LastUpdater { get; protected set; } = null!;

    public static bool operator ==(Entity? first, Entity? second)
    {
        return first is not null && second is not null && first.Equals(second);
    }

    public static bool operator !=(Entity? first, Entity? second)
    {
        return !(first == second);
    }

    public bool Equals(Entity? other)
    {
        if (other is null)
            return false;

        if (other.GetType() != GetType())
            return false;

        return other.Id == Id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (obj.GetType() != GetType())
            return false;

        if (obj is not Entity entity)
            return false;

        return entity.Id == Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode() * 79;
    }
}
