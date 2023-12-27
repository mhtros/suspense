using Suspense.Server.Models;

namespace Suspense.Server.Services;

/// <summary>
/// Represents a game manager responsible for handling game-related logic and interactions.
/// </summary>
public interface IGameManager
{
    /// <summary>
    /// Draws a card from the deck.
    /// </summary>
    public Task DrawCardAsync(string playerIds);

    /// <summary>
    /// Validates whether a given card move is valid within the game for a specific player.
    /// </summary>
    /// <param name="playerId">The ID of the player making the move.</param>
    /// <param name="move">The card move to validate.</param>
    /// <returns>True if the move is valid; otherwise, false.</returns>
    bool ValidateMove(string playerId, Card move);

    /// <summary>
    /// Allows a player to join the game.
    /// </summary>
    /// <param name="playerId">The unique identifier of the player.</param>
    /// <param name="isLeader">A flag indicating whether the player is the game leader.</param>
    /// <param name="connectionId">The connection ID of the player.</param>
    Task JoinGameAsync(string playerId, bool isLeader, string connectionId);

    /// <summary>
    /// Starts the game, initiated by the specified player.
    /// </summary>
    /// <param name="initiatorId">The unique identifier of the player who initiates the game.</param>
    Task StartGameAsync(string initiatorId);
}