namespace Suspense.Server.Models;

/// <summary>
/// Represents a playing card in a deck.
/// </summary>
public class Card
{
    /// <summary>
    /// Represents the rank or value of a playing card in a standard deck.
    /// </summary>
    public enum RankType
    {
        /// <summary>The Pass rank.</summary>
        Pass = -2,

        /// <summary>The Invalid rank. Use this rank ONLY for error handling scenarios.</summary>
        Invalid = -1,

        /// <summary>The Ace rank.</summary>
        Ace = 1,

        /// <summary>The Two rank.</summary>
        Two,

        /// <summary>The Three rank.</summary>
        Three,

        /// <summary>The Four rank.</summary>
        Four,

        /// <summary>The Five rank.</summary>
        Five,

        /// <summary>The Six rank.</summary>
        Six,

        /// <summary>The Seven rank.</summary>
        Seven,

        /// <summary>The Eight rank.</summary>
        Eight,

        /// <summary>The Nine rank.</summary>
        Nine,

        /// <summary>The Ten rank.</summary>
        Ten,

        /// <summary>The Jack rank.</summary>
        Jack,

        /// <summary>The Queen rank.</summary>
        Queen,

        /// <summary>The King rank.</summary>
        King
    }

    /// <summary>
    /// Represents the suit of a playing card in a standard deck.
    /// </summary>
    public enum SuitType
    {
        /// <summary>The Pass suit.</summary>
        Pass = -2,

        /// <summary>The Invalid suit. Use this suit ONLY for error handling scenarios.</summary>
        Invalid = -1,

        /// <summary>The Diamonds suit.</summary>
        Diamonds = 1,

        /// <summary>The Clubs suit.</summary>
        Clubs,

        /// <summary>The Hearts suit.</summary>
        Hearts,

        /// <summary>The Spades suit.</summary>
        Spades
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Card"/> class.
    /// </summary>
    public Card()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Card"/> class.
    /// </summary>
    public Card(SuitType suit, RankType rank)
    {
        Suit = suit;
        Rank = rank;
    }

    /// <summary>
    /// Represents the rank or value of a playing card in a standard deck.
    /// </summary>
    public RankType Rank { get; set; }

    /// <summary>
    /// Represents the suit of a playing card in a standard deck.
    /// </summary>
    public SuitType Suit { get; set; }
}