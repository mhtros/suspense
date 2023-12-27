namespace Suspense.Server.Hubs;

/// <summary>
/// Defines the contract for server-side operations in a game-related SignalR hub.
/// </summary>
public interface IGameServerActions
{
    /// <summary>
    /// Joins a player to an existing game session.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game session to join.</param>
    /// <param name="playerId">The unique identifier of the player joining the game.</param>
    /// <param name="isLeader">If the current player is leader of the game.</param>
    Task JoinGame(string gameId, string playerId, bool isLeader);

    /// <summary>
    /// Initiates the start of a game by the designated leader within a game session.
    /// </summary>
    /// <param name="initiatorId">The unique identifier of the player that try to initiate the game.</param>
    /// <param name="gameId">The unique identifier of the game session to be started.</param>
    Task StartGame(string initiatorId, string gameId);
}