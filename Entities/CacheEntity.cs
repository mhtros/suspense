namespace Suspense.Server.Entities;

/// <summary>
/// Represents a base class for entities stored in an in-memory cache.
/// </summary>
public abstract class CacheEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CacheEntity" /> class with the specific unique identifier.
    /// </summary>
    /// <param name="guid">The unique identifier of the entity.</param>
    protected CacheEntity(Guid guid)
    {
        Id = guid.ToString();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheEntity" /> class.
    /// </summary>
    protected CacheEntity()
    {
    }

    /// <summary>
    /// The unique identifier of the entity.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Indicates the specific entity Type. This will be used to validate the raw json at cache deserialization.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;
}