namespace Suspense.Server.Entities;

/// <summary>
/// Represents a player in a game.
/// </summary>
public class Player : CacheEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Player" /> class with the specified name and unique identifier.
    /// </summary>
    /// <param name="name">The name of the player.</param>
    /// <param name="guid">The unique identifier of the player.</param>
    /// <param name="isBot">Value that indicates if the player is a bot.</param>
    public Player(string name, Guid guid, bool isBot = false) : base(guid)
    {
        Name = name;
        EntityType = nameof(Player);
        IsBot = isBot;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Player" /> class.
    /// </summary>
    public Player()
    {
    }

    /// <summary>
    /// Name of the player.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier of the player's game connection.
    /// This acts as the connector between <see cref="Player" /> and SignalR user session.
    /// </summary>
    public string? ConnectionId { get; set; }

    /// <summary>
    /// Identifies if the player is a bot.
    /// </summary>
    public bool IsBot { get; init; }
}