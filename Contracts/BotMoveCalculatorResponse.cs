using Suspense.Server.Models;

namespace Suspense.Server.Contracts;

/// <summary>
/// Represents the response from the bot's move calculation
/// </summary>
public class BotMoveCalculatorResponse
{
    /// <summary>
    /// Representing the bot's move.
    /// </summary>
    public Card Move { get; private set; } = null!;

    /// <summary>
    /// This can be used to save the bot suit response if it decides to play <see cref="Card.RankType.Ace"/>.
    /// </summary>
    public Card.SuitType? SuitModificator { get; set; }

    /// <summary>
    /// Sets the move for the bot and returns the updated object instance.
    /// </summary>
    /// <param name="move">The card representing the bot's move.</param>
    /// <returns>The updated <see cref="BotMoveCalculatorResponse"/> with the new move.</returns>
    public BotMoveCalculatorResponse SetMoveAndReturnResponse(Card move)
    {
        Move = move;
        return this;
    }
}