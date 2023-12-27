using Suspense.Server.Models;

namespace Suspense.Server.Entities;

/// <summary>
/// Represents the state of a game.
/// </summary>
public class GameState : CacheEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameState" /> class with the specified unique identifier.
    /// </summary>
    /// <param name="guid">The unique identifier of the game.</param>
    public GameState(Guid guid) : base(guid)
    {
        EntityType = nameof(GameState);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameState" /> class.
    /// </summary>
    public GameState()
    {
    }

    /// <summary>
    /// Maximum number of turns allowed in the game.
    /// </summary>
    public int TurnLimit { get; set; }

    /// <summary>
    /// Current turn number.
    /// </summary>
    public int CurrentTurn { get; set; }

    /// <summary>
    /// Number of cards left in the deck. <br/>
    /// Unfortunately we cannot use Deck.Count because deck will clear before broadcasting.
    /// </summary>
    public int CardsLeft { get; set; }

    /// <summary>
    /// Collection of players data in the game. The String part represents the Id of each player.
    /// </summary>
    public Dictionary<string, PlayerData> PlayersData { get; set; } = new();

    /// <summary>
    /// Collection of cards in the deck that are not dealt yet.
    /// </summary>
    public List<Card> Deck { get; set; } = new();

    /// <summary>
    /// Collection of cards that have been played.
    /// </summary>
    public List<Card> PlayedCards { get; set; } = new();


    /// <summary>
    /// Represents the modified suit type.
    /// </summary>
    /// <remarks>
    /// This property enables to track the correct suit value
    /// when a player changed it by playing an <see cref="Card.RankType.Ace"/> card.
    /// </remarks>
    public Card.SuitType? SuitModificator { get; set; }
}