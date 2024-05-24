using Suspense.Server.Contracts;
using Suspense.Server.Models;

namespace Suspense.Server.Services;

/// <summary>
/// Class responsible for calculating the bot's move in a game.
/// </summary>
public interface IBotMoveCalculator
{
    /// <summary>
    /// Calculates the bot's move.
    /// </summary>
    /// <param name="data">The player's data, including their hand and other game information.</param>
    /// <param name="playedCards">The cards that have been played so far.</param>
    /// <param name="nextHandCount">The count of cards in the next hand.</param>
    /// <param name="is1Vs1">Indicates if the game is a one versus one match.</param>
    /// <param name="suitModificator">Indicates if the previous player have changed the suit.</param>
    /// <param name="drawCardAsync">An asynchronous function to draw a card from the deck.</param>
    public Task<BotMoveCalculatorResponse> CalculateMoveAsync(PlayerData data, IList<Card> playedCards,
        int nextHandCount, bool is1Vs1, Card.SuitType? suitModificator, Func<string, Task> drawCardAsync);
}