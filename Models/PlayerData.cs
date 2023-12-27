using Suspense.Server.Entities;

namespace Suspense.Server.Models;

/// <summary>
/// Represents data associated with a player.
/// </summary>
public class PlayerData
{
    /// <summary>
    /// The <see cref="Player"/> object representing the player.
    /// </summary>
    public Player Player { get; set; } = null!;

    /// <summary>
    /// The list of <see cref="Card"/> objects representing the cards in the player's hand.
    /// </summary>
    public List<Card> Hand { get; set; } = new();

    /// <summary>
    /// Player's score.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Number of cards left in a hand. <br/>
    /// Unfortunately we cannot use Hand.Count because hand will clear before broadcasting.
    /// </summary>
    public int CardsLeft { get; set; }

    /// <summary>
    /// Value indicating whether the current player is the leader.
    /// </summary>
    public bool IsLeader { get; set; }

    /// <summary>
    /// Value indicating whether it is the turn of the current players.
    /// </summary>
    public bool IsHisTurn { get; set; }

    /// <summary>
    /// Value indicating whether the player has drawn a card.
    /// </summary>
    public bool HasDraw { get; set; }

    /// <summary>
    /// Value indicating whether the player has lost their turn.
    /// </summary>
    public bool LostHisTurn { get; set; }

    /// <summary>
    /// Count of penalty cards associated with the player.
    /// </summary>
    public int PenaltyCardsCount { get; set; }
}